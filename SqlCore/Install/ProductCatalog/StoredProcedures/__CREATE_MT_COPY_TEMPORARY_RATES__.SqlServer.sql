
CREATE PROCEDURE mt_copy_tempopary_rates (
	@id_sched_key uniqueidentifier
)
AS
	SET NOCOUNT ON

	DECLARE @id_rate int
	DECLARE @id_rate_new int
	DECLARE rates CURSOR FOR
		SELECT id_rate
		FROM   #tmp_rates

	OPEN rates
	FETCH NEXT FROM rates INTO @id_rate

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #tmp_schedule_rates
				(id_sched, id_sched_key, id_audit, n_order, updated)
		SELECT id_sched, @id_sched_key, id_audit, n_order, updated
		FROM   #tmp_rates
		WHERE  id_rate = @id_rate

		SELECT @id_rate_new = SCOPE_IDENTITY()
				
		INSERT INTO #tmp_schedule_rate_params
					(id_rate, id_param, nm_param)
		SELECT @id_rate_new, id_param, nm_param
		FROM   #tmp_schedule_rate_params
		WHERE  id_rate = @id_rate

		FETCH NEXT FROM rates INTO @id_rate
	END

	CLOSE rates
	DEALLOCATE rates
