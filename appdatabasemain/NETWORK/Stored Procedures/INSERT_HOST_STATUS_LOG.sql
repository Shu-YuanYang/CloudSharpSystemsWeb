CREATE PROCEDURE [NETWORK].[INSERT_HOST_STATUS_LOG] 
	@HOST_IP VARCHAR(20), 
	@HOST_STATUS VARCHAR(20), 
	@TRACE_ID VARCHAR(100), 
	@INPUT_MESSAGE VARCHAR(500), 
	@EDIT_BY VARCHAR(100),
	@LATENCY FLOAT = 0
AS
BEGIN

	DECLARE @app_ID VARCHAR(50) = 'CloudSharpSystemsWeb';

	DECLARE @prev_error_rate FLOAT;
	DECLARE @prev_record_time DATETIME;
	
	DECLARE @cutoff_days FLOAT = 30.0;
	DECLARE @estimation_date DATETIME = GETDATE();
	DECLARE @cutoff_date DATETIME = DATEADD(DAY,-@cutoff_days, @estimation_date);

	-- 1. Query most recent error rate and record time, disregard records older than 30 days: 
	/*
	SELECT TOP 1 @prev_error_rate = ERROR_RATE, @prev_record_time = EDIT_TIME 
	FROM NETWORK.TB_HOST_STATUS_LOG
	WHERE HOST_IP = @HOST_IP AND EDIT_TIME > @cutoff_date
	ORDER BY EDIT_TIME DESC;
	*/
	SELECT TOP 1 @prev_error_rate = ERROR_RATE, @prev_record_Time = MEASURED_TIME
	FROM NETWORK.TB_WEBSITE_HOST
	WHERE HOST_IP = @HOST_IP AND [STATUS] <> 'DISABLED' AND MEASURED_TIME > @cutoff_date
	ORDER BY MEASURED_TIME DESC;

	SET @prev_error_rate = COALESCE(@prev_error_rate, 0);
	SET @prev_record_time = COALESCE(@prev_record_time, @cutoff_date);


	-- 2. If new record is 'Good', reduce error rate, and if new record is 'Error', increase error rate::
	DECLARE @cutoff_seconds FLOAT = @cutoff_days * 24 * 60 * 60;
	DECLARE @age FLOAT = DATEDIFF(SECOND, @prev_record_time, @estimation_date);
	DECLARE @scale FLOAT = SQRT( (@cutoff_seconds - @age) / @cutoff_seconds );
	
	DECLARE @Alpha FLOAT = (
		SELECT TOP 1 CONVERT(FLOAT, CONTROL_VALUE) FROM APPLICATIONS.V_APP_DATA_CONTROL 
		WHERE APP_ID = @app_ID AND CONTROL_NAME = 'SYSTEM_HEALTH_ANALYZER' AND CONTROL_TYPE = 'ERROR_ANALYSIS_CONFIG' AND CONTROL_LEVEL = 'EWMA_ERROR_WEIGHT'
		AND IS_APP_ENABLED = 'Y' AND IS_CONTROL_ENABLED = 'Y'
	);
	DECLARE @Beta_0 FLOAT = 1 - @Alpha;
	DECLARE @Beta FLOAT = @Beta_0 * @scale;
	
	DECLARE @new_error_rate FLOAT = @Beta * @prev_error_rate + (1 - @Beta) * (CASE @HOST_STATUS WHEN 'NORMAL' THEN 0 ELSE 1 END);
	IF @new_error_rate < 0.0001 SET @new_error_rate = 0; -- Shu-Yuan Yang 11132023 added a lower bound.

	-- 3. Insert new record in the host status log table:
	INSERT INTO NETWORK.TB_HOST_STATUS_LOG
	SELECT 
		FORMAT(GETDATE(), 'yyyyMMddHHmmss') + '_' + CONVERT([nvarchar](50), NEWID()) AS LOG_ID,
		@HOST_IP AS HOST_IP,
		@HOST_STATUS AS HOST_STATUS,
		@TRACE_ID AS TRACE_ID,
		(CASE @HOST_STATUS WHEN 'NORMAL' THEN 'GOOD' ELSE 'ERROR' END) AS RECORD_TYPE,
		@INPUT_MESSAGE AS RECORD_MESSAGE,
		@new_error_rate AS ERROR_RATE,
		@EDIT_BY AS EDIT_BY,
		@estimation_date AS EDIT_TIME,
		@LATENCY AS LATENCY;


	-- 4. Update error rate record 
	UPDATE NETWORK.TB_WEBSITE_HOST SET ERROR_MEASUREMENT_ALGORITHM = 'EWMA', ERROR_RATE = @new_error_rate, MEASURED_TIME = @estimation_date, EDIT_BY = @EDIT_BY, EDIT_TIME = GETDATE()
	WHERE HOST_IP = @HOST_IP;

END;