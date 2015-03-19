
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO
if exists (select 1 from sys.objects where name = 'MTMinOfThreeDates' and type = 'FN')
   drop function MTMinOfThreeDates
PRINT N'CREATE FUNCTION MTMinOfThreeDates'
GO
CREATE FUNCTION MTMinOfThreeDates
(
	@date1  DATETIME,
	@date2  DATETIME,
	@date3  DATETIME
)
RETURNS DATETIME
BEGIN
	RETURN dbo.MTMinOfTwoDates(@date1, dbo.MTMinOfTwoDates(@date2, @date3))
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
if exists (select 1 from sys.objects where name = 'MTMaxOfThreeDates' and type = 'FN')
	drop function MTMaxOfTHreeDates
PRINT N'CREATE FUNCTION MTMaxOfThreeDates'
GO
CREATE FUNCTION MTMaxOfThreeDates
(
	@date1  DATETIME,
	@date2  DATETIME,
	@date3  DATETIME
)
RETURNS DATETIME
BEGIN
	RETURN dbo.MTMaxOfTwoDates(@date1, dbo.MTMaxOfTwoDates(@date2, @date3))
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
if exists (select 1 from sys.objects where name = 'GetCurrentAccountCycleRange' and type = 'P')
	drop procedure GetCurrentAccountCycleRange

PRINT N'CREATE PROCEDURE GetCurrentAccountCycleRange'
GO
CREATE PROCEDURE GetCurrentAccountCycleRange
   @id_acc INT,
   @curr_date DATETIME,
   @StartCycle DATETIME OUTPUT,
   @EndCycle DATETIME OUTPUT
AS
  SELECT @StartCycle = dt_start,
         @EndCycle = dt_end
  FROM   t_usage_interval ui
         JOIN t_acc_usage_cycle acc
              ON  ui.id_usage_cycle = acc.id_usage_cycle
  WHERE  acc.id_acc = @id_acc
         AND @curr_date BETWEEN dt_start AND dt_end;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO

PRINT N'ALTER TRIGGER trig_update_t_recur_window_with_t_payment_redirection'
GO
ALTER TRIGGER trig_update_t_recur_window_with_t_payment_redirection
ON t_payment_redirection FOR INSERT, DELETE
/* Trigger always processing a single change related to update of 1 date range of 1 payee */
AS
BEGIN
  IF @@ROWCOUNT = 0 RETURN;

  DECLARE @currentDate DATETIME;
  SELECT @currentDate = MAX(h.tt_start)
  FROM   INSERTED i
         JOIN t_payment_redir_history h
              ON  h.id_payee = i.id_payee
              AND h.id_payer = i.id_payer
              AND h.tt_end = dbo.MTMaxDate();

  IF EXISTS (SELECT * FROM DELETED)
  BEGIN
    /* Create shared table, if it wasn't created yet by another session */
    IF OBJECT_ID('tempdb..##tmp_redir_deleted') IS NULL 
      SELECT @@SPID spid, * INTO ##tmp_redir_deleted FROM t_payment_redirection WHERE 1=0;

    INSERT INTO ##tmp_redir_deleted SELECT @@SPID spid, * FROM DELETED;    
    RETURN;
  END;

  IF EXISTS (SELECT * FROM INSERTED)
  BEGIN
    IF OBJECT_ID('tempdb..##tmp_redir_deleted') IS NULL
      RETURN;
    IF NOT EXISTS (SELECT * FROM ##tmp_redir_deleted WHERE spid = @@SPID)
      RETURN; /* This is not Payer update. This is account creation. */

    /* Skip rows, that are the same.
       Skip new date ranges, if they are inside old range of the same payer. (was already billed)  */
    SELECT *
    INTO  #tmp_new_payer_range
    FROM  INSERTED new
    WHERE NOT EXISTS (
          SELECT 1
          FROM   ##tmp_redir_deleted old
          WHERE  spid = @@SPID
             AND old.id_payer = new.id_payer
             AND old.id_payee = new.id_payee
             /* Same or inside old range */
             AND old.vt_start <= new.vt_start AND new.vt_end <= old.vt_end
          )

    SELECT  new.id_payee Payee,
            new.id_payer NewPayer,
            old.id_payer OldPayer,
            new.vt_start BillRangeStart,
            new.vt_end BillRangeEnd,
            /* Date ranges of old and new payer. Requires for joins to rw and business rules validation. */
            new.vt_start NewStart,
            new.vt_end NewEnd,
            old.vt_start OldStart,
            old.vt_end OldEnd,
            '=' NewRangeInsideOld
          INTO #tmp_redir
    FROM   #tmp_new_payer_range new
          JOIN ##tmp_redir_deleted old
            ON old.id_payee = new.id_payee AND old.id_payer <> new.id_payer
              AND new.vt_start = old.vt_start AND old.vt_end = new.vt_end /* Old range the same as new one */

    IF NOT EXISTS ( SELECT 1 FROM #tmp_redir)
    BEGIN
      INSERT INTO #tmp_redir
      SELECT  new.id_payee Payee,
              new.id_payer NewPayer,
              old.id_payer OldPayer,
              new.vt_start BillRangeStart,
              new.vt_end BillRangeEnd,
              /* Date ranges of old and new payer. Requires for joins to rw and business rules validation. */
              new.vt_start NewStart,
              new.vt_end NewEnd,
              old.vt_start OldStart,
              old.vt_end OldEnd,
              'Y' NewRangeInsideOld
      FROM   #tmp_new_payer_range new
            JOIN ##tmp_redir_deleted old
              ON old.id_payee = new.id_payee AND old.id_payer <> new.id_payer
                AND old.vt_start <= new.vt_start AND new.vt_end <= old.vt_end /* New range inside old one */
      UNION
      SELECT  new.id_payee Payee,
              new.id_payer NewPayer,
              old.id_payer OldPayer,
              old.vt_start BillRangeStart,
              old.vt_end BillRangeEnd,
              /* Date ranges of old and new payer. Requires for joins to rw and business rules validation. */
              new.vt_start NewStart,
              new.vt_end NewEnd,
              old.vt_start OldStart,
              old.vt_end OldEnd,
              'N' NewRangeInsideOld
      FROM   #tmp_new_payer_range new
            JOIN ##tmp_redir_deleted old
              ON old.id_payee = new.id_payee AND old.id_payer <> new.id_payer
                AND new.vt_start <= old.vt_start AND old.vt_end <= new.vt_end /* Old range inside new one */
    END;

    /* Clean-up temp data of my session */
    DELETE FROM ##tmp_redir_deleted WHERE spid = @@SPID;
  END;

  /* Double-check that we detected payer change. */
  IF NOT EXISTS ( SELECT 1 FROM #tmp_redir)
    THROW 50000,'Fail to retrieve payer change information. #tmp_redir is empty.',1

  DECLARE @oldPayerCycleStart DATETIME,
          @oldPayerCycleEnd DATETIME,
          @oldPayerStart DATETIME,
          @oldPayerEnd DATETIME,
          @oldPayerId INT,
          @newPayerStart DATETIME,
          @newPayerEnd DATETIME,
          @newPayerId INT

  SELECT TOP 1 @newPayerStart = NewStart, @newPayerEnd = NewEnd, @newPayerId = NewPayer,
               @oldPayerStart = OldStart, @oldPayerEnd = OldEnd, @oldPayerId = OldPayer
  FROM #tmp_redir;
  
  EXEC GetCurrentAccountCycleRange @id_acc = @oldPayerId, @curr_date = @currentDate,
                                   @StartCycle = @oldPayerCycleStart OUT, @EndCycle = @oldPayerCycleEnd OUT;
  /* Check for current limitations */
  IF @oldPayerCycleStart <= @newPayerStart AND @newPayerEnd <= @oldPayerCycleEnd
    THROW 50000,'Limitation: New payer cannot start and end in current billing cycle.',1

  /* TODO: Check limitation "Payer starts before current interval start"
         This is not working in some scenarios. Not proper check - correct this.
  IF @newPayerStart <= @oldPayerCycleStart
    THROW 50000,'Limitation: New payer cannot start before the start of current billing interval.',1    
  IF @newPayerEnd <= @oldPayerCycleEnd
    THROW 50000,'Limitation: New payer cannot end before the start of next billing interval.',1
  */
  /* TODO: Check scenario where new payer replaces > 1 old payer */

  /* Snapshot current recur window, that will be used as template */
  SELECT trw.c_CycleEffectiveDate,
         trw.c_CycleEffectiveStart,
         trw.c_CycleEffectiveEnd,
         trw.c_SubscriptionStart,
         trw.c_SubscriptionEnd,
         trw.c_Advance,
         trw.c__AccountID,
         trw.c__PayingAccount,
         trw.c__PriceableItemInstanceID,
         trw.c__PriceableItemTemplateID,
         trw.c__ProductOfferingID,
         trw.c_PayerStart,
         trw.c_PayerEnd,
         trw.c__SubscriptionID,
         trw.c_UnitValueStart,
         trw.c_UnitValueEnd,
         trw.c_UnitValue,
         trw.c_BilledThroughDate,
         trw.c_LastIdRun,
         trw.c_MembershipStart,
         trw.c_MembershipEnd,
         dbo.AllowInitialArrersCharge(trw.c_Advance, trw.c__PayingAccount, trw.c_SubscriptionEnd, @currentDate, 0 ) AS c__IsAllowGenChargeByTrigger
         INTO #old_rw
  FROM   t_recur_window trw
         JOIN #tmp_redir r
              ON  trw.c__AccountID = r.Payee
              AND trw.c__PayingAccount = r.OldPayer
              AND trw.c_PayerStart = r.OldStart
              AND trw.c_PayerEnd = r.OldEnd;

  /* Populate recur window for date range of new payer, to charge new payer and refund old payer */
  SELECT r.BillRangeStart,
         r.BillRangeEnd,
         rw.c_CycleEffectiveDate,
         rw.c_CycleEffectiveStart,
         rw.c_CycleEffectiveEnd,
         rw.c_SubscriptionStart,
         rw.c_SubscriptionEnd,
         rw.c_Advance,
         rw.c__AccountID,
         @newPayerId AS c__PayingAccount_New,
         @oldPayerId AS c__PayingAccount_Old, /* Temp additional field. Is used only for metering 'Credit' charges below. */
         rw.c__PriceableItemInstanceID,
         rw.c__PriceableItemTemplateID,
         rw.c__ProductOfferingID,
         dbo.MTMinOfTwoDates(@oldPayerStart, @newPayerStart) AS c_PayerStart, /* Get maximum Payer range for charging. */
         dbo.MTMaxOfTwoDates(@newPayerEnd, @oldPayerEnd) AS c_PayerEnd,
         rw.c__SubscriptionID,
         rw.c_UnitValueStart,
         rw.c_UnitValueEnd,
         rw.c_UnitValue,
         rw.c_BilledThroughDate,
         rw.c_LastIdRun,
         rw.c_MembershipStart,
         rw.c_MembershipEnd,
         rw.c__IsAllowGenChargeByTrigger
         INTO #recur_window_holder
  FROM   #old_rw rw
         JOIN #tmp_redir r
              ON  rw.c__AccountID = r.Payee
              AND rw.c__PayingAccount = r.OldPayer;

  /* TODO: Fix scenarios:
           1. Account is paid by P1 and P2 in future. Updating so that P2 is paying for all period. */
  EXEC MeterPayerChangesFromRecurWindow @currentDate;

  /* Update payer dates in t_recur_window */

  /* If ranges of Old and New payer are the same - just update Payer ID */
  IF EXISTS (SELECT * FROM #tmp_redir WHERE NewRangeInsideOld = '=')
  BEGIN
    UPDATE rw
    SET    c__PayingAccount = @newPayerId
    FROM   t_recur_window rw
           JOIN #old_rw orw
              ON rw.c__ProductOfferingID = orw.c__ProductOfferingID
              AND rw.c__SubscriptionID = orw.c__SubscriptionID
    WHERE  rw.c__PayingAccount = @oldPayerId
           AND rw.c_PayerStart = @newPayerStart
           AND rw.c_PayerEnd = @newPayerEnd;
  END
  /* If New payer range inside Old payer range:
     1. Update Old Payer Dates;
     2. Insert New Payer recur window.*/
  ELSE IF EXISTS (SELECT * FROM #tmp_redir WHERE NewRangeInsideOld = 'Y')
  BEGIN
    /* TODO: Handle case when new Payer does not have common Start Or End with Old payer. */
    IF EXISTS (SELECT * FROM t_recur_window rw
               JOIN #old_rw orw
                  ON  rw.c__PayingAccount = orw.c__PayingAccount AND rw.c__ProductOfferingID = orw.c__ProductOfferingID
                  AND rw.c_PayerStart = orw.c_PayerStart AND rw.c_PayerEnd = orw.c_PayerEnd
                  AND rw.c__SubscriptionID = orw.c__SubscriptionID
               WHERE  orw.c_PayerStart <> @newPayerStart AND orw.c_PayerEnd <> @newPayerEnd)
      THROW 50000,'Limitation: New and Old payer ranges should either have a common start date or common end date.',1

    /* Update Old Payer Dates */
    /* Old payer now ends just before new payer start, if new payer after old one. */
    UPDATE rw
    SET    c_PayerEnd = dbo.SubtractSecond(@newPayerStart)
    FROM   t_recur_window rw
           JOIN #old_rw orw
              ON  rw.c__PayingAccount = orw.c__PayingAccount
              AND rw.c__ProductOfferingID = orw.c__ProductOfferingID
              AND rw.c_PayerStart = orw.c_PayerStart
              AND rw.c_PayerEnd = orw.c_PayerEnd
              AND rw.c__SubscriptionID = orw.c__SubscriptionID
    WHERE  orw.c_PayerStart <> @newPayerStart;
    /* TODO: replace WHERE with "orw.c_PayerEnd = @newPayerEnd;"
    OR: IF (@newPayerEnd = @oldPayerEnd) THEN... */

    /* Old payer now starts right after new payer ends, if new payer before old one. */
    UPDATE rw
    SET    c_PayerStart = dbo.AddSecond(@newPayerEnd)
    FROM   t_recur_window rw
           JOIN #old_rw orw
              ON  rw.c__PayingAccount = orw.c__PayingAccount
              AND rw.c__ProductOfferingID = orw.c__ProductOfferingID
              AND rw.c_PayerStart = orw.c_PayerStart
              AND rw.c_PayerEnd = orw.c_PayerEnd
              AND rw.c__SubscriptionID = orw.c__SubscriptionID
    WHERE  orw.c_PayerEnd <> @newPayerEnd;
    /* TODO: replace WHERE with "orw.c_PayerStart = @newPayerStart;"
    OR: IF (@newPayerStart = @oldPayerStart) THEN... */

    /* Insert New Payer recur window */
    INSERT INTO t_recur_window
    SELECT DISTINCT c_CycleEffectiveDate,
           c_CycleEffectiveStart,
           c_CycleEffectiveEnd,
           c_SubscriptionStart,
           c_SubscriptionEnd,
           c_Advance,
           c__AccountID,
           c__PayingAccount_New,
           c__PriceableItemInstanceID,
           c__PriceableItemTemplateID,
           c__ProductOfferingID,
           @newPayerStart AS c_PayerStart,
           @newPayerEnd AS c_PayerEnd,
           c__SubscriptionID,
           c_UnitValueStart,
           c_UnitValueEnd,
           c_UnitValue,
           c_BilledThroughDate,
           c_LastIdRun,
           c_MembershipStart,
           c_MembershipEnd
    FROM   #recur_window_holder;
  END
  /* If Old payer range inside New payer range:
     1. Delete Old Payer range from recur window;
     2. Update New Payer Dates. */
  ELSE IF EXISTS (SELECT * FROM #tmp_redir WHERE NewRangeInsideOld = 'N')
  BEGIN
    /* TODO: Handle case when new Payer does not have common Start Or End with Old payer. */
    IF EXISTS (SELECT * FROM t_recur_window rw
               JOIN #old_rw orw
                  ON  rw.c__PayingAccount = orw.c__PayingAccount AND rw.c__ProductOfferingID = orw.c__ProductOfferingID
                  AND rw.c_PayerStart = orw.c_PayerStart AND rw.c_PayerEnd = orw.c_PayerEnd
                  AND rw.c__SubscriptionID = orw.c__SubscriptionID
               WHERE  orw.c_PayerStart <> @newPayerStart AND orw.c_PayerEnd <> @newPayerEnd)
      THROW 50000,'Limitation: New and Old payer ranges should either have a common start date or common end date.',1

    DELETE
    FROM   t_recur_window
    WHERE  EXISTS (SELECT 1 FROM #old_rw orw
                   WHERE  t_recur_window.c__PayingAccount = @oldPayerId
                          AND t_recur_window.c_PayerStart >= @newPayerStart
                          AND t_recur_window.c_PayerEnd <= @newPayerEnd
                          AND t_recur_window.c__ProductOfferingID = orw.c__ProductOfferingID
                          AND t_recur_window.c__SubscriptionID = orw.c__SubscriptionID
                  );
    UPDATE rw
    SET    c_PayerStart = @newPayerStart,
           c_PayerEnd = @newPayerEnd
    FROM   t_recur_window rw
           JOIN #old_rw orw
              ON rw.c__ProductOfferingID = orw.c__ProductOfferingID
              AND rw.c__SubscriptionID = orw.c__SubscriptionID
    WHERE  rw.c__PayingAccount = @newPayerId
           AND (rw.c_PayerStart = @newPayerStart OR rw.c_PayerEnd = @newPayerEnd)
  END
  ELSE
    THROW 50000,'Unable to determine is new payer range inside old payer, vice-versa or they are the same.',1

  /* TODO: Do we need this UPDATE? */
  UPDATE t_recur_window
  SET    c_CycleEffectiveEnd = (
             SELECT MIN(ISNULL(c_CycleEffectiveDate, c_SubscriptionEnd))
             FROM   t_recur_window w2
             WHERE  w2.c__SubscriptionId = t_recur_window.c__SubscriptionId
                    AND t_recur_window.c_PayerStart = w2.c_PayerStart
                    AND t_recur_window.c_PayerEnd = w2.c_PayerEnd
                    AND t_recur_window.c_UnitValueStart = w2.c_UnitValueStart
                    AND t_recur_window.c_UnitValueEnd = w2.c_UnitValueEnd
                    AND t_recur_window.c_membershipstart = w2.c_membershipstart
                    AND t_recur_window.c_membershipend = w2.c_membershipend
                    AND t_recur_window.c__accountid = w2.c__accountid
                    AND t_recur_window.c__payingaccount = w2.c__payingaccount
                    AND w2.c_CycleEffectiveDate > t_recur_window.c_CycleEffectiveDate
         )
  WHERE  c__PayingAccount IN (SELECT c__PayingAccount_New FROM #recur_window_holder)
         AND EXISTS
             (
                 SELECT 1
                 FROM   t_recur_window w2
                 WHERE  w2.c__SubscriptionId = t_recur_window.c__SubscriptionId
                        AND t_recur_window.c_PayerStart = w2.c_PayerStart
                        AND t_recur_window.c_PayerEnd = w2.c_PayerEnd
                        AND t_recur_window.c_UnitValueStart = w2.c_UnitValueStart
                        AND t_recur_window.c_UnitValueEnd = w2.c_UnitValueEnd
                        AND t_recur_window.c_membershipstart = w2.c_membershipstart
                        AND t_recur_window.c_membershipend = w2.c_membershipend
                        AND t_recur_window.c__accountid = w2.c__accountid
                        AND t_recur_window.c__payingaccount = w2.c__payingaccount
                        AND w2.c_CycleEffectiveDate > t_recur_window.c_CycleEffectiveDate
             );
END;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO

PRINT N'ALTER PROCEDURE MeterPayerChangesFromRecurWindow'
GO
ALTER PROCEDURE MeterPayerChangesFromRecurWindow
  @currentDate datetime
AS
BEGIN
  SET NOCOUNT ON;

  IF ((SELECT value FROM t_db_values WHERE parameter = N'InstantRc') = 'false') RETURN;

  SELECT 'InitialDebit'                                                                             AS c_RCActionType,
         pci.dt_start                                                                               AS c_RCIntervalStart,
         pci.dt_end                                                                                 AS c_RCIntervalEnd,
         ui.dt_start                                                                                AS c_BillingIntervalStart,
         ui.dt_end                                                                                  AS c_BillingIntervalEnd,
         dbo.MTMaxOfThreeDates(BillRangeStart, pci.dt_start, rw.c_SubscriptionStart)                AS c_RCIntervalSubscriptionStart,
         dbo.MTMinOfThreeDates(BillRangeEnd, pci.dt_end, rw.c_SubscriptionEnd)                      AS c_RCIntervalSubscriptionEnd,
         rw.c_SubscriptionStart                                                                     AS c_SubscriptionStart,
         rw.c_SubscriptionEnd                                                                       AS c_SubscriptionEnd,
         dbo.MTMinOfTwoDates(pci.dt_end, rw.c_SubscriptionEnd)                                      AS c_BilledRateDate,
         rcr.n_rating_type                                                                          AS c_RatingType,
         CASE WHEN rw.c_advance = 'Y' THEN '1' ELSE '0' END                                         AS c_Advance,
         '1'                                                                                        AS c_ProrateOnSubscription,
         '1'                                                                                        AS c_ProrateInstantly, /* NOTE: c_ProrateInstantly - No longer used */
         '1'                                                                                        AS c_ProrateOnUnsubscription,
         CASE WHEN rcr.b_fixed_proration_length = 'Y' THEN fxd.n_proration_length ELSE 0 END        AS c_ProrationCycleLength,
         rw.c__accountid                                                                            AS c__AccountID,
         rw.c__PayingAccount_New                                                                    AS c__PayingAccount,         
         rw.c__priceableiteminstanceid                                                              AS c__PriceableItemInstanceID,
         rw.c__priceableitemtemplateid                                                              AS c__PriceableItemTemplateID,
         rw.c__productofferingid                                                                    AS c__ProductOfferingID,
         rw.c_UnitValueStart                                                                        AS c_UnitValueStart,
         rw.c_UnitValueEnd                                                                          AS c_UnitValueEnd,
         rw.c_UnitValue                                                                             AS c_UnitValue,
         ui.id_interval                                                                             AS c__IntervalID,
         rw.c__subscriptionid                                                                       AS c__SubscriptionID,
         NEWID()                                                                                    AS idSourceSess,
         sub.tx_quoting_batch                                                                       AS c__QuoteBatchId
  INTO #tmp_rc
  FROM   t_usage_interval ui
         INNER JOIN #recur_window_holder rw
              ON  rw.c_payerstart          < ui.dt_end AND rw.c_payerend          > ui.dt_start /* next interval overlaps with payer */
              AND rw.c_cycleeffectivestart < ui.dt_end AND rw.c_cycleeffectiveend > ui.dt_start /* next interval overlaps with cycle */
              AND rw.c_membershipstart     < ui.dt_end AND rw.c_membershipend     > ui.dt_start /* next interval overlaps with membership */
              AND rw.c_SubscriptionStart   < ui.dt_end AND rw.c_SubscriptionEnd   > ui.dt_start
              AND rw.c_unitvaluestart      < ui.dt_end AND rw.c_unitvalueend      > ui.dt_start /* next interval overlaps with UDRC */
         INNER LOOP JOIN t_recur rcr ON rw.c__priceableiteminstanceid = rcr.id_prop         
         INNER LOOP JOIN t_acc_usage_cycle auc ON auc.id_acc = rw.c__PayingAccount_New AND auc.id_usage_cycle = ui.id_usage_cycle
         INNER LOOP JOIN t_usage_cycle ccl
              ON  ccl.id_usage_cycle = CASE 
                                        WHEN rcr.tx_cycle_mode = 'Fixed' THEN rcr.id_usage_cycle
                                        WHEN rcr.tx_cycle_mode = 'BCR Constrained' THEN ui.id_usage_cycle 
                                        WHEN rcr.tx_cycle_mode = 'EBCR' THEN dbo.DeriveEBCRCycle(ui.id_usage_cycle, rw.c_SubscriptionStart, rcr.id_cycle_type) 
                                        ELSE NULL
                                       END
         INNER LOOP JOIN t_usage_cycle_type fxd ON fxd.id_cycle_type = ccl.id_cycle_type
         /* NOTE: we do not join RC interval by id_interval.  It is different (not sure what the reasoning is) */
         INNER LOOP JOIN t_pc_interval pci WITH(INDEX(cycle_time_pc_interval_index)) ON pci.id_cycle = ccl.id_usage_cycle
              AND (
                      pci.dt_start  BETWEEN ui.dt_start AND ui.dt_end                          /* Check if rc start falls in this interval */
                      OR pci.dt_end BETWEEN ui.dt_start AND ui.dt_end                          /* or check if the cycle end falls into this interval */
                      OR (pci.dt_start < ui.dt_start AND pci.dt_end > ui.dt_end)               /* or this interval could be in the middle of the cycle */
                  )
              AND pci.dt_end BETWEEN rw.c_payerstart AND rw.c_payerend                            /* rc start goes to this payer */              
              AND rw.c_unitvaluestart      < pci.dt_end AND rw.c_unitvalueend      > pci.dt_start /* rc overlaps with this UDRC */
              AND rw.c_membershipstart     < pci.dt_end AND rw.c_membershipend     > pci.dt_start /* rc overlaps with this membership */
              AND rw.c_cycleeffectivestart < pci.dt_end AND rw.c_cycleeffectiveend > pci.dt_start /* rc overlaps with this cycle */
              AND rw.c_SubscriptionStart   < pci.dt_end AND rw.c_subscriptionend   > pci.dt_start /* rc overlaps with this subscription */
         INNER JOIN t_usage_interval currentui ON rw.c_SubscriptionStart BETWEEN currentui.dt_start AND currentui.dt_end
              AND currentui.id_usage_cycle = ui.id_usage_cycle
         INNER JOIN t_sub sub on sub.id_sub = rw.c__SubscriptionID
  WHERE
         @currentDate BETWEEN ui.dt_start AND ui.dt_end /* TODO: Support Backdated subscriptions. Works only with current interval for now. */
         AND rw.c__IsAllowGenChargeByTrigger = 1 /* TODO: Remove this */

  UNION ALL

  SELECT 'InitialCredit'                                                                            AS c_RCActionType,
         pci.dt_start                                                                               AS c_RCIntervalStart,
         pci.dt_end                                                                                 AS c_RCIntervalEnd,
         ui.dt_start                                                                                AS c_BillingIntervalStart,
         ui.dt_end                                                                                  AS c_BillingIntervalEnd,
         dbo.MTMaxOfThreeDates(BillRangeStart, pci.dt_start, rw.c_SubscriptionStart)                AS c_RCIntervalSubscriptionStart,
         dbo.MTMinOfThreeDates(BillRangeEnd, pci.dt_end, rw.c_SubscriptionEnd)                      AS c_RCIntervalSubscriptionEnd,
         rw.c_SubscriptionStart                                                                     AS c_SubscriptionStart,
         rw.c_SubscriptionEnd                                                                       AS c_SubscriptionEnd,
         dbo.MTMinOfTwoDates(pci.dt_end, rw.c_SubscriptionEnd)                                      AS c_BilledRateDate,
         rcr.n_rating_type                                                                          AS c_RatingType,
         CASE WHEN rw.c_advance = 'Y' THEN '1' ELSE '0' END                                         AS c_Advance,
         '1'                                                                                        AS c_ProrateOnSubscription,
         '1'                                                                                        AS c_ProrateInstantly, /* NOTE: c_ProrateInstantly - No longer used */
         '1'                                                                                        AS c_ProrateOnUnsubscription,
         CASE WHEN rcr.b_fixed_proration_length = 'Y' THEN fxd.n_proration_length ELSE 0 END        AS c_ProrationCycleLength,
         rw.c__accountid                                                                            AS c__AccountID,
         rw.c__PayingAccount_Old                                                                    AS c__PayingAccount,         
         rw.c__priceableiteminstanceid                                                              AS c__PriceableItemInstanceID,
         rw.c__priceableitemtemplateid                                                              AS c__PriceableItemTemplateID,
         rw.c__productofferingid                                                                    AS c__ProductOfferingID,
         rw.c_UnitValueStart                                                                        AS c_UnitValueStart,
         rw.c_UnitValueEnd                                                                          AS c_UnitValueEnd,
         rw.c_UnitValue                                                                             AS c_UnitValue,
         ui.id_interval                                                                             AS c__IntervalID,
         rw.c__subscriptionid                                                                       AS c__SubscriptionID,
         NEWID()                                                                                    AS idSourceSess,
         sub.tx_quoting_batch                                                                       as c__QuoteBatchId
  FROM   t_usage_interval ui
         INNER JOIN #recur_window_holder rw
              ON  rw.c_payerstart          < ui.dt_end AND rw.c_payerend          > ui.dt_start /* next interval overlaps with payer */
              AND rw.c_cycleeffectivestart < ui.dt_end AND rw.c_cycleeffectiveend > ui.dt_start /* next interval overlaps with cycle */
              AND rw.c_membershipstart     < ui.dt_end AND rw.c_membershipend     > ui.dt_start /* next interval overlaps with membership */
              AND rw.c_SubscriptionStart   < ui.dt_end AND rw.c_SubscriptionEnd   > ui.dt_start
              AND rw.c_unitvaluestart      < ui.dt_end AND rw.c_unitvalueend      > ui.dt_start /* next interval overlaps with UDRC */
         INNER LOOP JOIN t_recur rcr ON rw.c__priceableiteminstanceid = rcr.id_prop         
         INNER LOOP JOIN t_acc_usage_cycle auc ON auc.id_acc = rw.c__PayingAccount_Old AND auc.id_usage_cycle = ui.id_usage_cycle
         INNER LOOP JOIN t_usage_cycle ccl
              ON  ccl.id_usage_cycle = CASE 
                                        WHEN rcr.tx_cycle_mode = 'Fixed' THEN rcr.id_usage_cycle
                                        WHEN rcr.tx_cycle_mode = 'BCR Constrained' THEN ui.id_usage_cycle 
                                        WHEN rcr.tx_cycle_mode = 'EBCR' THEN dbo.DeriveEBCRCycle(ui.id_usage_cycle, rw.c_SubscriptionStart, rcr.id_cycle_type) 
                                        ELSE NULL
                                       END
         INNER LOOP JOIN t_usage_cycle_type fxd ON fxd.id_cycle_type = ccl.id_cycle_type
         /* NOTE: we do not join RC interval by id_interval.  It is different (not sure what the reasoning is) */
         INNER LOOP JOIN t_pc_interval pci WITH(INDEX(cycle_time_pc_interval_index)) ON pci.id_cycle = ccl.id_usage_cycle
              AND (
                      pci.dt_start  BETWEEN ui.dt_start AND ui.dt_end                          /* Check if rc start falls in this interval */
                      OR pci.dt_end BETWEEN ui.dt_start AND ui.dt_end                          /* or check if the cycle end falls into this interval */
                      OR (pci.dt_start < ui.dt_start AND pci.dt_end > ui.dt_end)               /* or this interval could be in the middle of the cycle */
                  )
              AND pci.dt_end BETWEEN rw.c_payerstart AND rw.c_payerend                            /* rc start goes to this payer */              
              AND rw.c_unitvaluestart      < pci.dt_end AND rw.c_unitvalueend      > pci.dt_start /* rc overlaps with this UDRC */
              AND rw.c_membershipstart     < pci.dt_end AND rw.c_membershipend     > pci.dt_start /* rc overlaps with this membership */
              AND rw.c_cycleeffectivestart < pci.dt_end AND rw.c_cycleeffectiveend > pci.dt_start /* rc overlaps with this cycle */
              AND rw.c_SubscriptionStart   < pci.dt_end AND rw.c_subscriptionend   > pci.dt_start /* rc overlaps with this subscription */
         INNER JOIN t_usage_interval currentui ON rw.c_SubscriptionStart BETWEEN currentui.dt_start AND currentui.dt_end
              AND currentui.id_usage_cycle = ui.id_usage_cycle
         INNER JOIN t_sub sub on sub.id_sub = rw.c__SubscriptionID
  WHERE
         @currentDate BETWEEN ui.dt_start AND ui.dt_end /* TODO: Support Backdated subscriptions. Works only with current interval for now. */
         AND rw.c__IsAllowGenChargeByTrigger = 1; /* TODO: Remove this */

  /* Clean-up charges, that are out of BillRange */
  DELETE FROM #tmp_rc WHERE c_RCIntervalSubscriptionEnd < c_RCIntervalSubscriptionStart;

  /* If no charges to meter, return immediately */
  IF NOT EXISTS (SELECT 1 FROM #tmp_rc) RETURN;

  EXEC InsertChargesIntoSvcTables;

/* BilledThroughDates should be left the same after payer change. */
/* BUT we might need it on payer change to diff. cycle. */
/*
  MERGE
  INTO    #recur_window_holder trw
  USING   (
            SELECT MAX(c_RCIntervalSubscriptionEnd) AS NewBilledThroughDate, c__AccountID, c__ProductOfferingID, c__PriceableItemInstanceID, c__PriceableItemTemplateID, c_RCActionType, c__SubscriptionID
            FROM #tmp_rc
            WHERE c_RCActionType = 'InitialDebit'
            GROUP BY c__AccountID, c__ProductOfferingID, c__PriceableItemInstanceID, c__PriceableItemTemplateID, c_RCActionType, c__SubscriptionID
          ) trc
  ON      (
            trw.c__AccountID = trc.c__AccountID
            AND trw.c__SubscriptionID = trc.c__SubscriptionID
            AND trw.c__PriceableItemInstanceID = trc.c__PriceableItemInstanceID
            AND trw.c__PriceableItemTemplateID = trc.c__PriceableItemTemplateID
            AND trw.c__ProductOfferingID = trc.c__ProductOfferingID
            AND trw.c__IsAllowGenChargeByTrigger = 1
          )
  WHEN MATCHED THEN
  UPDATE
  SET     trw.c_BilledThroughDate = trc.NewBilledThroughDate;
*/
END;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO

PRINT N'ALTER PROCEDURE [dbo].[mtsp_generate_stateful_rcs]'
GO
ALTER PROCEDURE [dbo].[mtsp_generate_stateful_rcs]
                                            @v_id_interval  int
                                           ,@v_id_billgroup int
                                           ,@v_id_run       int
                                           ,@v_id_batch     varchar(256)
                                           ,@v_n_batch_size int
                                                               ,@v_run_date   datetime
                                           ,@p_count      int OUTPUT
AS
BEGIN
  /* SET NOCOUNT ON added to prevent extra result sets from
     interfering with SELECT statements. */
  SET NOCOUNT ON;
  SET XACT_ABORT ON;
  DECLARE @total_rcs  int,
          @total_flat int,
          @total_udrc int,
          @n_batches  int,
          @id_flat    int,
          @id_udrc    int,
          @id_message bigint,
          @id_ss      int,
          @tx_batch   binary(16);

  TRUNCATE TABLE t_rec_win_bcp_for_reverse;

  INSERT INTO t_rec_win_bcp_for_reverse (c_BilledThroughDate, c_CycleEffectiveDate, c__PriceableItemInstanceID, c__PriceableItemTemplateID, c__ProductOfferingID, c__SubscriptionID) 
  SELECT c_BilledThroughDate, c_CycleEffectiveDate, c__PriceableItemInstanceID, c__PriceableItemTemplateID, c__ProductOfferingID, c__SubscriptionID
  FROM t_recur_window;


INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Retrieving RC candidates');
SELECT
*
INTO
#TMP_RC
FROM(
	SELECT
      'Arrears'                                                                            AS c_RCActionType
      ,pci.dt_start                                                                        AS c_RCIntervalStart
      ,pci.dt_end                                                                          AS c_RCIntervalEnd
      ,ui.dt_start                                                                         AS c_BillingIntervalStart
      ,ui.dt_end                                                                           AS c_BillingIntervalEnd
      ,dbo.MTMaxOfThreeDates(rw.c_payerstart, pci.dt_start, rw.c_SubscriptionStart)        AS c_RCIntervalSubscriptionStart
      ,dbo.MTMinOfThreeDates(rw.c_payerend, pci.dt_end, rw.c_SubscriptionEnd)              AS c_RCIntervalSubscriptionEnd
      ,rw.c_SubscriptionStart                                                              AS c_SubscriptionStart
      ,rw.c_SubscriptionEnd                                                                AS c_SubscriptionEnd
      ,pci.dt_end                                                                          AS c_BilledRateDate
      ,rcr.n_rating_type                                                                   AS c_RatingType
      ,case when rw.c_advance  ='Y' then '1' else '0' end                                  AS c_Advance
      ,case when rcr.b_prorate_on_activate ='Y'
                 or (rw.c_payerstart BETWEEN ui.dt_start AND ui.dt_end AND rw.c_payerstart > rw.c_SubscriptionStart)
            then '1'
            else '0' end                                                                   AS c_ProrateOnSubscription
      ,case when rcr.b_prorate_instantly  ='Y' then '1' else '0' end                       AS c_ProrateInstantly /* NOTE: c_ProrateInstantly - No longer used */
      ,case when rcr.b_prorate_on_deactivate ='Y'
                 or (rw.c_payerend BETWEEN ui.dt_start AND ui.dt_end AND rw.c_payerend < rw.c_SubscriptionEnd)
            then '1'
            else '0' end                                                                   AS c_ProrateOnUnsubscription
      ,CASE WHEN rcr.b_fixed_proration_length = 'Y' THEN fxd.n_proration_length ELSE 0 END AS c_ProrationCycleLength
      ,rw.c__accountid                                                                     AS c__AccountID
      ,rw.c__payingaccount                                                                 AS c__PayingAccount
      ,rw.c__priceableiteminstanceid                                                       AS c__PriceableItemInstanceID
      ,rw.c__priceableitemtemplateid                                                       AS c__PriceableItemTemplateID
      ,rw.c__productofferingid                                                             AS c__ProductOfferingID
      ,rw.c_payerstart                                                                     AS c_payerstart
      ,rw.c_payerend                                                                       AS c_payerend
      ,case when rw.c_unitvaluestart < '1970-01-01 00:00:00'
         THEN '1970-01-01 00:00:00'
         ELSE rw.c_unitvaluestart END                                                      AS c_unitvaluestart 
      ,rw.c_unitvalueend                                                                   AS c_unitvalueend
      ,rw.c_unitvalue                                                                      AS c_unitvalue
      ,rw.c__subscriptionid                                                                AS c__SubscriptionID
      ,newid()                                                                             AS idSourceSess
      FROM t_usage_interval ui      
      INNER LOOP JOIN t_billgroup bg ON bg.id_usage_interval = ui.id_interval
      INNER LOOP JOIN t_billgroup_member bgm ON bg.id_billgroup = bgm.id_billgroup      
      INNER LOOP JOIN t_recur_window rw WITH(INDEX(rc_window_time_idx)) ON bgm.id_acc = rw.c__payingaccount 
                                   AND rw.c_payerstart          < ui.dt_end AND rw.c_payerend          > ui.dt_start /* interval overlaps with payer */
                                   AND rw.c_cycleeffectivestart < ui.dt_end AND rw.c_cycleeffectiveend > ui.dt_start /* interval overlaps with cycle */
                                   AND rw.c_membershipstart     < ui.dt_end AND rw.c_membershipend     > ui.dt_start /* interval overlaps with membership */
                                   AND rw.c_subscriptionstart   < ui.dt_end AND rw.c_subscriptionend   > ui.dt_start /* interval overlaps with subscription */
                                   AND rw.c_unitvaluestart      < ui.dt_end AND rw.c_unitvalueend      > ui.dt_start /* interval overlaps with UDRC */
      INNER LOOP JOIN t_recur rcr ON rw.c__priceableiteminstanceid = rcr.id_prop      
      INNER LOOP JOIN t_usage_cycle ccl
           ON ccl.id_usage_cycle = CASE
                                         WHEN rcr.tx_cycle_mode = 'Fixed'           THEN rcr.id_usage_cycle
                                         WHEN rcr.tx_cycle_mode = 'BCR Constrained' THEN ui.id_usage_cycle
                                         WHEN rcr.tx_cycle_mode = 'EBCR'            THEN dbo.DeriveEBCRCycle(ui.id_usage_cycle, rw.c_SubscriptionStart, rcr.id_cycle_type)
                                         ELSE NULL
                                   END
      INNER LOOP JOIN t_usage_cycle_type fxd ON fxd.id_cycle_type = ccl.id_cycle_type
      /* NOTE: we do not join RC interval by id_interval.  It is different (not sure what the reasoning is) */
      INNER LOOP JOIN t_pc_interval pci WITH(INDEX(cycle_time_pc_interval_index)) ON pci.id_cycle = ccl.id_usage_cycle
                                   AND pci.dt_end BETWEEN ui.dt_start        AND ui.dt_end                             /* rc end falls in this interval */
                                   AND (
                                      pci.dt_end BETWEEN rw.c_payerstart  AND rw.c_payerend	/* rc start goes to this payer */
                                      OR ( /* rc end or overlaps this payer */
                                          pci.dt_end >= rw.c_payerstart
                                          AND pci.dt_start < rw.c_payerend
                                        )
                                   )
                                   AND rw.c_unitvaluestart      < pci.dt_end AND rw.c_unitvalueend      > pci.dt_start /* rc overlaps with this UDRC */
                                   AND rw.c_membershipstart     < pci.dt_end AND rw.c_membershipend     > pci.dt_start /* rc overlaps with this membership */
                                   AND rw.c_cycleeffectivestart < pci.dt_end AND rw.c_cycleeffectiveend > pci.dt_start /* rc overlaps with this cycle */
                                   AND rw.c_SubscriptionStart   < pci.dt_end AND rw.c_subscriptionend   > pci.dt_start /* rc overlaps with this subscription */
      WHERE
        ui.id_interval = @v_id_interval
        AND bg.id_billgroup = @v_id_billgroup
        AND rcr.b_advance <> 'Y'
 /* Exclude any accounts which have been billed through the charge range.
	     This is because they will have been billed through to the end of last period (advanced charged)
		 OR they will have ended their subscription in which case all of the charging has been done.
		 ONLY subscriptions which are scheduled to end this period which have not been ended by subscription change will be caught 
		 in these queries
		 */
      AND rw.c_BilledThroughDate < dbo.mtmaxoftwodates(pci.dt_start, rw.c_SubscriptionStart)
      /* CORE-8365. If Subscription started and ended in this Bill.cycle, than this is an exception case, when Arrears are generated by trigger.
      Do not charge them here, in EOP. */
      AND NOT (rw.c_SubscriptionStart >= ui.dt_start AND rw.c_SubscriptionEnd <= ui.dt_end)
UNION ALL
SELECT
      'Advance'                                                                            AS c_RCActionType
      ,pci.dt_start		                                                                     AS c_RCIntervalStart		/* Start date of Next RC Interval - the one we'll pay for In Advance in current interval */
      ,pci.dt_end		                                                                       AS c_RCIntervalEnd			/* End date of Next RC Interval - the one we'll pay for In Advance in current interval */
      ,ui.dt_start		                                                                     AS c_BillingIntervalStart	/* Start date of Current Billing Interval */
      ,ui.dt_end	                                                                      	 AS c_BillingIntervalEnd		/* End date of Current Billing Interval */
      ,CASE WHEN rcr.tx_cycle_mode <> 'Fixed' AND nui.dt_start <> c_cycleEffectiveDate 
         THEN dbo.MTMaxOfThreeDates(rw.c_payerstart, dbo.AddSecond(c_cycleEffectiveDate), pci.dt_start)
         ELSE dbo.MTMaxOfThreeDates(rw.c_payerstart, pci.dt_start, rw.c_SubscriptionStart)
       END                                                                                 AS c_RCIntervalSubscriptionStart
      ,dbo.MTMinOfThreeDates(rw.c_payerend, pci.dt_end, rw.c_SubscriptionEnd)              AS c_RCIntervalSubscriptionEnd
      ,rw.c_SubscriptionStart                                                              AS c_SubscriptionStart
      ,rw.c_SubscriptionEnd                                                                AS c_SubscriptionEnd
      ,pci.dt_start                                                                        AS c_BilledRateDate
      ,rcr.n_rating_type                                                                   AS c_RatingType
      ,case when rw.c_advance  ='Y' then '1' else '0' end                                  AS c_Advance
      ,case when rcr.b_prorate_on_activate ='Y'
                 or rw.c_payerstart BETWEEN nui.dt_start AND nui.dt_end
            then '1'
            else '0' end                                                                   AS c_ProrateOnSubscription
      ,case when rcr.b_prorate_instantly  ='Y' then '1' else '0' end                       AS c_ProrateInstantly /* NOTE: c_ProrateInstantly - No longer used */
      ,case when rcr.b_prorate_on_deactivate ='Y'
                 or rw.c_payerend BETWEEN nui.dt_start AND nui.dt_end
            then '1'
            else '0' end                                                                   AS c_ProrateOnUnsubscription
      ,CASE WHEN rcr.b_fixed_proration_length = 'Y' THEN fxd.n_proration_length ELSE 0 END AS c_ProrationCycleLength
      ,rw.c__accountid                                                                     AS c__AccountID
      ,rw.c__payingaccount                                                                 AS c__PayingAccount
      ,rw.c__priceableiteminstanceid                                                       AS c__PriceableItemInstanceID
      ,rw.c__priceableitemtemplateid                                                       AS c__PriceableItemTemplateID
      ,rw.c__productofferingid                                                             AS c__ProductOfferingID
      ,rw.c_payerstart                                                                     AS c_payerstart
      ,rw.c_payerend                                                                       AS c_payerend
      ,case when rw.c_unitvaluestart < '1970-01-01 00:00:00'
         THEN '1970-01-01 00:00:00'
         ELSE rw.c_unitvaluestart END                                                      AS c_unitvaluestart 
      ,rw.c_unitvalueend                                                                   AS c_unitvalueend
      ,rw.c_unitvalue                                                                      AS c_unitvalue
      ,rw.c__subscriptionid                                                                AS c__SubscriptionID
      ,newid()                                                                             AS idSourceSess
      FROM t_usage_interval ui
      INNER LOOP JOIN t_usage_interval nui ON ui.id_usage_cycle = nui.id_usage_cycle AND dbo.AddSecond(ui.dt_end) = nui.dt_start
      INNER LOOP JOIN t_billgroup bg ON bg.id_usage_interval = ui.id_interval
      INNER LOOP JOIN t_billgroup_member bgm ON bg.id_billgroup = bgm.id_billgroup      
      INNER LOOP JOIN t_recur_window rw WITH(INDEX(rc_window_time_idx)) ON bgm.id_acc = rw.c__payingaccount 
                                   AND rw.c_payerstart          < nui.dt_end AND rw.c_payerend          > nui.dt_start /* next interval overlaps with payer */
                                   AND rw.c_cycleeffectivestart < nui.dt_end AND rw.c_cycleeffectiveend > nui.dt_start /* next interval overlaps with cycle */
                                   AND rw.c_membershipstart     < nui.dt_end AND rw.c_membershipend     > nui.dt_start /* next interval overlaps with membership */
                                   AND rw.c_subscriptionstart   < nui.dt_end AND rw.c_subscriptionend   > nui.dt_start /* next interval overlaps with subscription */
                                   AND rw.c_unitvaluestart      < nui.dt_end AND rw.c_unitvalueend      > nui.dt_start /* next interval overlaps with UDRC */
      INNER LOOP JOIN t_recur rcr ON rw.c__priceableiteminstanceid = rcr.id_prop      
      INNER LOOP JOIN t_usage_cycle ccl
           ON ccl.id_usage_cycle = CASE
                                         WHEN rcr.tx_cycle_mode = 'Fixed'           THEN rcr.id_usage_cycle
                                         WHEN rcr.tx_cycle_mode = 'BCR Constrained' THEN ui.id_usage_cycle
                                         WHEN rcr.tx_cycle_mode = 'EBCR'            THEN dbo.DeriveEBCRCycle(ui.id_usage_cycle, rw.c_SubscriptionStart, rcr.id_cycle_type)
                                         ELSE NULL
                                   END
      INNER LOOP JOIN t_usage_cycle_type fxd ON fxd.id_cycle_type = ccl.id_cycle_type
      INNER LOOP JOIN t_pc_interval pci WITH(INDEX(cycle_time_pc_interval_index)) ON pci.id_cycle = ccl.id_usage_cycle
                                   AND (
                                      pci.dt_start BETWEEN nui.dt_start AND nui.dt_end /* RCs that starts in Next Account's Billing Cycle */
                                      
                                      /* Fix for CORE-7060:
                                      In case subscription starts after current EOP we should also charge:
                                      RCs that ends in Next Account's Billing Cycle
                                      and if Next Account's Billing Cycle in the middle of RCs interval.
                                      As in this case, they haven't been charged as Instant RC (by trigger) */
                                      OR (
                                          rw.c_SubscriptionStart >= nui.dt_start
                                          AND pci.dt_end >= nui.dt_start
                                          AND pci.dt_start < nui.dt_end
                                        )
                                   )
                                   AND (
                                      pci.dt_start BETWEEN rw.c_payerstart  AND rw.c_payerend	/* rc start goes to this payer */
                                      
                                      /* Fix for CORE-7273:
                                      Logic above, that relates to Account Billing Cycle, should be duplicated for Payer's Billing Cycle.
                                      
                                      CORE-7273 related case: If Now = EOP = Subscription Start then:
                                      1. Not only RC's that starts in this payer's cycle should be charged, but also the one, that ends and overlaps it;
                                      2. Proration wasn't calculated by trigger and should be done by EOP. */
                                      OR (
                                          pci.dt_end >= rw.c_payerstart
                                          AND pci.dt_start < rw.c_payerend
                                        )
                                   )                                   
                                   AND rw.c_unitvaluestart		< pci.dt_end AND rw.c_unitvalueend      > pci.dt_start /* rc overlaps with this UDRC */
                                   AND rw.c_membershipstart		< pci.dt_end AND rw.c_membershipend     > pci.dt_start /* rc overlaps with this membership */
                                   AND rw.c_cycleeffectiveend	> pci.dt_start /* rc overlaps with this cycle */
                                   AND rw.c_subscriptionend		> pci.dt_start /* rc overlaps with this subscription */
      WHERE
        ui.id_interval = @v_id_interval
        AND bg.id_billgroup = @v_id_billgroup
        AND rcr.b_advance = 'Y'
 /* Exclude any accounts which have been billed through the charge range.
	     This is because they will have been billed through to the end of last period (advanced charged)
		 OR they will have ended their subscription in which case all of the charging has been done.
		 ONLY subscriptions which are scheduled to end this period which have not been ended by subscription change will be caught 
		 in these queries
		 */
        AND rw.c_BilledThroughDate < dbo.mtmaxoftwodates(
                   (
                       CASE 
                           WHEN rcr.tx_cycle_mode <> 'Fixed' AND nui.dt_start <> c_cycleEffectiveDate 
                           THEN dbo.MTMaxOfTwoDates(dbo.AddSecond(c_cycleEffectiveDate), pci.dt_start) 
                           ELSE pci.dt_start END
                   ),
                   rw.c_SubscriptionStart
               )
)A;

/* Clean-up extra charges. May be caused by payer ranges overlap. */
DELETE FROM #TMP_RC WHERE c_RCIntervalSubscriptionEnd < c_RCIntervalSubscriptionStart;

SELECT @total_rcs  = COUNT(1) FROM #tmp_rc;

INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'RC Candidate Count: ' + CAST(@total_rcs AS VARCHAR));

if @total_rcs > 0
BEGIN

SELECT @total_flat = COUNT(1) FROM #tmp_rc where c_unitvalue is null;
SELECT @total_udrc = COUNT(1) FROM #tmp_rc where c_unitvalue is not null;

INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Flat RC Candidate Count: ' + CAST(@total_flat AS VARCHAR));
INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'UDRC RC Candidate Count: ' + CAST(@total_udrc AS VARCHAR));

INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Session Set Count: ' + CAST(@v_n_batch_size AS VARCHAR));
INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Batch: ' + @v_id_batch);

SELECT @tx_batch = cast(N'' as xml).value('xs:hexBinary(sql:variable("@v_id_batch"))', 'binary(16)');
INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Batch ID: ' + CAST(@tx_batch AS varchar));

IF (@tx_batch IS NOT NULL)
BEGIN
UPDATE t_batch SET n_metered = @total_rcs, n_expected = @total_rcs WHERE tx_batch = @tx_batch;
END;

if @total_flat > 0
begin

    
set @id_flat = (SELECT id_enum_data FROM t_enum_data ted WHERE ted.nm_enum_data =
      'metratech.com/flatrecurringcharge');
    
SET @n_batches = (@total_flat / @v_n_batch_size) + 1;
    EXEC GetIdBlock @n_batches, 'id_dbqueuesch', @id_message OUTPUT;
    EXEC GetIdBlock @n_batches, 'id_dbqueuess',  @id_ss OUTPUT;

INSERT INTO t_session 
(id_ss, id_source_sess)
SELECT @id_ss + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_ss,
    idSourceSess AS id_source_sess
FROM #tmp_rc where c_unitvalue is null;
         
INSERT INTO t_session_set
(id_message, id_ss, id_svc, b_root, session_count)
SELECT id_message, id_ss, id_svc, b_root, COUNT(1) as session_count
FROM
(SELECT @id_message + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_message,
    @id_ss + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_ss,
    @id_flat AS id_svc,
    1 AS b_root
FROM #tmp_rc
where c_unitvalue is null) a
GROUP BY a.id_message, a.id_ss, a.id_svc, a.b_root;

INSERT INTO t_svc_FlatRecurringCharge
(id_source_sess
    ,id_parent_source_sess
    ,id_external
    ,c_RCActionType
    ,c_RCIntervalStart
    ,c_RCIntervalEnd
    ,c_BillingIntervalStart
    ,c_BillingIntervalEnd
    ,c_RCIntervalSubscriptionStart
    ,c_RCIntervalSubscriptionEnd
    ,c_SubscriptionStart
    ,c_SubscriptionEnd
    ,c_Advance
    ,c_ProrateOnSubscription
    ,c_ProrateInstantly 
    ,c_ProrateOnUnsubscription
    ,c_ProrationCycleLength
    ,c__AccountID
    ,c__PayingAccount
    ,c__PriceableItemInstanceID
    ,c__PriceableItemTemplateID
    ,c__ProductOfferingID
    ,c_BilledRateDate
    ,c__SubscriptionID
    ,c__IntervalID
    ,c__Resubmit
    ,c__TransactionCookie
    ,c__CollectionID)
SELECT 
    idSourceSess AS id_source_sess
    ,NULL AS id_parent_source_sess
    ,NULL AS id_external
    ,c_RCActionType
    ,c_RCIntervalStart
    ,c_RCIntervalEnd
    ,c_BillingIntervalStart
    ,c_BillingIntervalEnd
    ,c_RCIntervalSubscriptionStart
    ,c_RCIntervalSubscriptionEnd
    ,c_SubscriptionStart
    ,c_SubscriptionEnd
    ,c_Advance
    ,c_ProrateOnSubscription
    ,c_ProrateInstantly 
    ,c_ProrateOnUnsubscription
    ,c_ProrationCycleLength
    ,c__AccountID
    ,c__PayingAccount
    ,c__PriceableItemInstanceID
    ,c__PriceableItemTemplateID
    ,c__ProductOfferingID
    ,c_BilledRateDate
    ,c__SubscriptionID
    ,@v_id_interval AS c__IntervalID
    ,'0' AS c__Resubmit
    ,NULL AS c__TransactionCookie
    ,@tx_batch AS c__CollectionID
FROM #tmp_rc
where c_unitvalue is null;
          INSERT
          INTO t_message
            (
              id_message,
              id_route,
              dt_crt,
              dt_metered,
              dt_assigned,
              id_listener,
              id_pipeline,
              dt_completed,
              id_feedback,
              tx_TransactionID,
              tx_sc_username,
              tx_sc_password,
              tx_sc_namespace,
              tx_sc_serialized,
              tx_ip_address
            )
            SELECT
              id_message,
              NULL,
              @v_run_date,
              @v_run_date,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              '127.0.0.1'
            FROM
              (SELECT @id_message + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_message
              FROM #tmp_rc
              WHERE c_unitvalue IS NULL
              ) a
            GROUP BY a.id_message;

INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Done inserting Flat RCs');

END;
if @total_udrc > 0
begin

set @id_udrc = (SELECT id_enum_data FROM t_enum_data ted WHERE ted.nm_enum_data =
      'metratech.com/udrecurringcharge');
    
SET @n_batches = (@total_udrc / @v_n_batch_size) + 1;
    EXEC GetIdBlock @n_batches, 'id_dbqueuesch', @id_message OUTPUT;
    EXEC GetIdBlock @n_batches, 'id_dbqueuess',  @id_ss OUTPUT;

INSERT INTO t_session 
(id_ss, id_source_sess)
SELECT @id_ss + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_ss,
    idSourceSess AS id_source_sess
FROM #tmp_rc where c_unitvalue is not null;
         
INSERT INTO t_session_set
(id_message, id_ss, id_svc, b_root, session_count)
SELECT id_message, id_ss, id_svc, b_root, COUNT(1) as session_count
FROM
(SELECT @id_message + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_message,
    @id_ss + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_ss,
    @id_udrc AS id_svc,
    1 AS b_root
FROM #tmp_rc
where c_unitvalue is not null) a
GROUP BY a.id_message, a.id_ss, a.id_svc, a.b_root;

INSERT INTO t_svc_UDRecurringCharge
(id_source_sess, id_parent_source_sess, id_external, c_RCActionType, c_RCIntervalStart,c_RCIntervalEnd,c_BillingIntervalStart,c_BillingIntervalEnd
    ,c_RCIntervalSubscriptionStart
    ,c_RCIntervalSubscriptionEnd
    ,c_SubscriptionStart
    ,c_SubscriptionEnd
    ,c_Advance
    ,c_ProrateOnSubscription
/*    ,c_ProrateInstantly */
    ,c_ProrateOnUnsubscription
    ,c_ProrationCycleLength
    ,c__AccountID
    ,c__PayingAccount
    ,c__PriceableItemInstanceID
    ,c__PriceableItemTemplateID
    ,c__ProductOfferingID
    ,c_BilledRateDate
    ,c__SubscriptionID
    ,c__IntervalID
    ,c__Resubmit
    ,c__TransactionCookie
    ,c__CollectionID
      ,c_unitvaluestart
      ,c_unitvalueend
      ,c_unitvalue
      ,c_ratingtype)
SELECT 
    idSourceSess AS id_source_sess
    ,NULL AS id_parent_source_sess
    ,NULL AS id_external
    ,c_RCActionType
    ,c_RCIntervalStart
    ,c_RCIntervalEnd
    ,c_BillingIntervalStart
    ,c_BillingIntervalEnd
    ,c_RCIntervalSubscriptionStart
    ,c_RCIntervalSubscriptionEnd
    ,c_SubscriptionStart
    ,c_SubscriptionEnd
    ,c_Advance
    ,c_ProrateOnSubscription
/*    ,c_ProrateInstantly */
    ,c_ProrateOnUnsubscription
    ,c_ProrationCycleLength
    ,c__AccountID
    ,c__PayingAccount
    ,c__PriceableItemInstanceID
    ,c__PriceableItemTemplateID
    ,c__ProductOfferingID
    ,c_BilledRateDate
    ,c__SubscriptionID
    ,@v_id_interval AS c__IntervalID
    ,'0' AS c__Resubmit
    ,NULL AS c__TransactionCookie
    ,@tx_batch AS c__CollectionID
      ,c_unitvaluestart
      ,c_unitvalueend
      ,c_unitvalue
      ,c_ratingtype
FROM #tmp_rc
where c_unitvalue is not null;

          INSERT
          INTO t_message
            (
              id_message,
              id_route,
              dt_crt,
              dt_metered,
              dt_assigned,
              id_listener,
              id_pipeline,
              dt_completed,
              id_feedback,
              tx_TransactionID,
              tx_sc_username,
              tx_sc_password,
              tx_sc_namespace,
              tx_sc_serialized,
              tx_ip_address
            )
            SELECT
              id_message,
              NULL,
              @v_run_date,
              @v_run_date,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              NULL,
              '127.0.0.1'
            FROM
              (SELECT @id_message + (ROW_NUMBER() OVER (ORDER BY idSourceSess) % @n_batches) AS id_message
              FROM #tmp_rc
              WHERE c_unitvalue IS NOT NULL
              ) a
            GROUP BY a.id_message;

                  INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Done inserting UDRC RCs');

END;
    /** UPDATE THE BILLED THROUGH DATE TO THE END OF THE ADVANCED CHARGE 
			 ** (IN CASE THE END THE SUB BEFORE THE END OF THE MONTH)
			 ** THIS WILL MAKE SURE THE CREDIT IS CORRECT AND MAKE SURE THERE ARE NOT CHARGES 
			 ** REGENERATED FOR ALL THE MONTHS WHERE RC ADAPTER RAN (But forgot to mark)
			 ** Only for advanced charges.
		     **/
    MERGE
    INTO    t_recur_window trw
    USING   (
              SELECT MAX(c_RCIntervalSubscriptionEnd) AS NewBilledThroughDate, c__AccountID, c__ProductOfferingID, c__PriceableItemInstanceID, c__PriceableItemTemplateID, c_RCActionType, c__SubscriptionID
              FROM #tmp_rc
              WHERE c_RCActionType = 'Advance'
              GROUP BY c__AccountID, c__ProductOfferingID, c__PriceableItemInstanceID, c__PriceableItemTemplateID, c_RCActionType, c__SubscriptionID
            ) trc
    ON      (
              trw.c__AccountID = trc.c__AccountID
              AND trw.c__SubscriptionID = trc.c__SubscriptionID
              AND trw.c__PriceableItemInstanceID = trc.c__PriceableItemInstanceID
              AND trw.c__PriceableItemTemplateID = trc.c__PriceableItemTemplateID
              AND trw.c__ProductOfferingID = trc.c__ProductOfferingID
            )
    WHEN MATCHED THEN
    UPDATE
    SET     trw.c_BilledThroughDate = trc.NewBilledThroughDate;

 END;

 SET @p_count = @total_rcs;

INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Info', 'Finished submitting RCs, count: ' + CAST(@total_rcs AS VARCHAR));

END;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO

COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO