
  CREATE TABLE %%PAYEE_SESSION%% (
	[id_acc] [int] NOT NULL,
	[id_dm_acc] [int] NOT NULL,
	[id_usage_interval] [int] NOT NULL,
	[id_prod] [int] NULL,
	[id_view] [int] NOT NULL,
	[id_pi_template] [int] NULL,
	[id_pi_instance] [int] NULL,
	[am_currency] [nvarchar] (3),
	[id_se] [int] NULL,
	[dt_session] [datetime] NULL,
	[TotalAmount] [numeric](38, 6) NULL,
	[TotalFederalTax] [numeric](38, 6) NULL,
	[TotalCountyTax] [numeric](38, 6) NULL,
	[TotalLocalTax] [numeric](38, 6) NULL,
	[TotalOtherTax] [numeric](38, 6) NULL,	
	[TotalStateTax] [numeric](38, 6) NULL,
	[TotalTax] [numeric](38, 6) NULL,
	[TotalImpliedTax] [numeric](38, 6) NULL,
    [TotalInformationalTax] [numeric](38, 6) NULL,
    [TotalImplInfTax] [numeric](38, 6) NULL,
	[PrebillAdjAmt] [numeric](38, 6) NULL,
	[PrebillFedTaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillStatetaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillCntytaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillLocaltaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillOthertaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillTotaltaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillImpliedTaxAdjAmt] [numeric](38, 6) NULL,
    [PrebillInformationalTaxAdjAmt] [numeric](38, 6) NULL,
    [PrebillImplInfTaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillAdjAmt] [numeric](38, 6) NULL,
	[PostbillFedTaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillStatetaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillCntytaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillLocaltaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillOthertaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillTotaltaxAdjAmt] [numeric](38, 6) NULL,
	[PostbillImpliedTaxAdjAmt] [numeric](38, 6) NULL,
    [PostbillInformationalTaxAdjAmt] [numeric](38, 6) NULL,
    [PostbillImplInfTaxAdjAmt] [numeric](38, 6) NULL,
	[PrebillAdjustedAmount] [numeric](38, 6) NULL,
	[PostbillAdjustedAmount] [numeric](38, 6) NULL,
	[NumPrebillAdjustments] [int] NULL,
	[NumPostbillAdjustments] [int] NULL,
	[NumTransactions] [int] NULL)
	CREATE CLUSTERED INDEX idx_%%PAYEE_SESSION%% ON %%PAYEE_SESSION%% (id_dm_acc, dt_session, id_view)
	create index idx2_%%PAYEE_SESSION%% on %%PAYEE_SESSION%% (id_dm_acc,id_acc,id_usage_interval,dt_session,id_se,id_view,am_currency)
	