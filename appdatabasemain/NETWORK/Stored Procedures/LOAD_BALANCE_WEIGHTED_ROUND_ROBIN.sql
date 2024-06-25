






CREATE PROCEDURE [NETWORK].[LOAD_BALANCE_WEIGHTED_ROUND_ROBIN] 
	@SITE_ID AS VARCHAR(100),
	@CLIENT_IP AS VARCHAR(20),
	@CLIENT_THREAD_ID AS VARCHAR(100),
	@RESOURCE_SIZE AS INT,
	@HOST_IP AS VARCHAR(100) OUTPUT,
	@RESOURCE_UNIT INT OUTPUT
	-- Created by Shu-Yuan Yang 09/20/2023
AS
BEGIN

	/******************************** MOVE TO PARENT PROCEDURE
	-- 0. Clear out abnormal sessions:
	EXEC NETWORK.VALIDATE_USER_SESSIONS @SITE_ID;


	-- 1. If client already has a connected session on a normally functioning host IP, return that host IP:
	SELECT TOP 1 @HOST_IP = TB_H.HOST_IP FROM NETWORK.TB_WEBSITE_HOST TB_H
	WHERE TB_H.SITE_ID = @SITE_ID AND TB_H.STATUS = 'NORMAL' AND TB_H.HOST_IP IN (
		SELECT TB_S.HOST_IP FROM NETWORK.TB_USER_SESSION TB_S 
		WHERE TB_S.CLIENT_IP = @CLIENT_IP AND 
			  TB_S.THREAD_ID = @CLIENT_THREAD_ID AND
			  TB_S.IS_VALID = 'Y'
	);
	IF @HOST_IP IS NOT NULL BEGIN
		SET @RESOURCE_UNIT = -1;
		RETURN;
	END
	*/


	-- 2. Get algorithm data settings:
	DECLARE @app_ID VARCHAR(50) = 'CloudSharpSystemsWeb';
	DECLARE @data_control_name VARCHAR(50) = 'LOAD_BALANCING_ALGORITHM';
	DECLARE @algorithm_type VARCHAR(50) = 'WEIGHTED_ROUND_ROBIN';
	DECLARE @weight_scale FLOAT = 0.1;

	DECLARE @current_max_weight INT;
	DECLARE @current_load INT;
	DECLARE @current_index INT;

	DECLARE @DATA_CONTROL TABLE(CONTROL_LEVEL VARCHAR(50), CONTROL_VALUE VARCHAR(250));
	
	INSERT INTO @DATA_CONTROL
	SELECT CONTROL_LEVEL, CONTROL_VALUE FROM APPLICATIONS.V_APP_DATA_CONTROL with (nolock) 
	WHERE CONTROL_NAME = @data_control_name AND APP_ID = @app_ID AND 
		  CONTROL_TYPE = @algorithm_type AND IS_APP_ENABLED = 'Y' AND IS_CONTROL_ENABLED = 'Y'
	
	SELECT @current_index = CONVERT(INT, CONTROL_VALUE) FROM @DATA_CONTROL WHERE CONTROL_LEVEL = 'CURRENT_INDEX';
	SELECT @current_max_weight = CONVERT(INT, CONTROL_VALUE) FROM @DATA_CONTROL WHERE CONTROL_LEVEL = 'CURRENT_MAX_WEIGHT';
	SELECT @current_load = CONVERT(INT, CONTROL_VALUE) FROM @DATA_CONTROL WHERE CONTROL_LEVEL = 'CURRENT_LOAD';
	
	--print @current_index;
	--print @current_max_weight;
	--print @current_load;


	-- 3. Select host IP based on load distribution:
	-- 3.1 If the current max weight can still handle the new input resource size, select the host IP of the current index.
	IF @current_load < @current_max_weight BEGIN
		SELECT @HOST_IP = HOST_IP FROM (
			SELECT *, ROW_NUMBER() OVER (ORDER BY SERIAL_NO, NET_LOAD_CAPACITY DESC) AS rn FROM NETWORK.GET_SERVER_LOAD(@SITE_ID) 
			WHERE [NETWORK].[IS_HOST_RESPONSIVE](SERVER_STATUS, IP_STATUS) = 'Y' -- Shu-Yuan Yang 11142023 modified to use function to determine responsive status 
		) cte WHERE rn = @current_index AND NET_LOAD_CAPACITY - RESOURCE_LOAD > @RESOURCE_SIZE;

		SET @current_load = @current_load + @RESOURCE_SIZE;	
	END 
		
	-- 3.2 If the current max weight cannot allocate for the new resource input, find the next host IP that has enough capacity.
	IF @HOST_IP IS NULL BEGIN
		WITH LOAD_DIST AS (
			SELECT * FROM (select *, ROW_NUMBER() over (order by SERIAL_NO, NET_LOAD_CAPACITY desc) as rn from NETWORK.GET_SERVER_LOAD(@SITE_ID) 
			-- Shu-Yuan Yang 09292023 added to avoid selecting disconnected app servers.
			WHERE [NETWORK].[IS_HOST_RESPONSIVE](SERVER_STATUS, IP_STATUS) = 'Y') cte  -- Shu-Yuan Yang 11142023 modified to use function to determine responsive status
		),
		NEXT_HOST_CANDIDATE AS (
			SELECT TOP 1 * FROM LOAD_DIST WHERE rn > @current_index AND NET_LOAD_CAPACITY - RESOURCE_LOAD > @RESOURCE_SIZE ORDER BY SERIAL_NO, NET_LOAD_CAPACITY DESC
			UNION
			SELECT TOP 1 * FROM LOAD_DIST WHERE rn <= @current_index AND NET_LOAD_CAPACITY - RESOURCE_LOAD > @RESOURCE_SIZE ORDER BY SERIAL_NO, NET_LOAD_CAPACITY DESC
		)	
		SELECT TOP 1 @HOST_IP = HOST_IP, @current_index = rn, @current_max_weight = (NET_LOAD_CAPACITY * @weight_scale) FROM NEXT_HOST_CANDIDATE ORDER BY rn DESC;

		SET @current_load = @RESOURCE_SIZE;	
	END

	-- 3.3 If a host IP is found to be able to handle the incoming session request, update round robin load balancer cache data: 
	IF @HOST_IP IS NOT NULL BEGIN
		--SET @current_load = @current_load + @RESOURCE_SIZE;
		SET @RESOURCE_UNIT = 1;
		
		UPDATE APPLICATIONS.TB_APP_DATA_CONTROL SET CONTROL_VALUE = CONVERT(VARCHAR(150), @current_index)
		WHERE CONTROL_NAME = @data_control_name AND APP_ID = @app_ID AND 
			  CONTROL_TYPE = @algorithm_type AND CONTROL_LEVEL = 'CURRENT_INDEX' AND IS_ENABLED = 'Y';

		UPDATE APPLICATIONS.TB_APP_DATA_CONTROL SET CONTROL_VALUE = CONVERT(VARCHAR(150), @current_max_weight)
		WHERE CONTROL_NAME = @data_control_name AND APP_ID = @app_ID AND 
			  CONTROL_TYPE = @algorithm_type AND CONTROL_LEVEL = 'CURRENT_MAX_WEIGHT' AND IS_ENABLED = 'Y';

		UPDATE APPLICATIONS.TB_APP_DATA_CONTROL SET CONTROL_VALUE = CONVERT(VARCHAR(150), @current_load)
		WHERE CONTROL_NAME = @data_control_name AND APP_ID = @app_ID AND 
			  CONTROL_TYPE = @algorithm_type AND CONTROL_LEVEL = 'CURRENT_LOAD' AND IS_ENABLED = 'Y';
	END

END;
