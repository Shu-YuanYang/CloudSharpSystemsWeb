﻿

CREATE PROCEDURE [NETWORK].[VALIDATE_USER_SESSIONS]
	@SITE_ID AS VARCHAR(100)
	-- Created by Shu-Yuan Yang 09/20/2023
	-- Determine each user session validity by server host status. Invalidate sessions for abnormal host status and clear the session data.
AS
BEGIN
	DECLARE @ProblemHostIP TABLE(HOST_IP VARCHAR(20));

	WITH TB_H AS (SELECT H.* FROM NETWORK.TB_WEBSITE_HOST H WHERE H.SITE_ID = @SITE_ID),
	TB_SER AS (SELECT * FROM PRODUCTS.TB_SERVER S WHERE S.SERIAL_NO IN (SELECT TB_H.SERIAL_NO FROM TB_H)),
	TB_PROBLEM_SER AS (
		SELECT TB_H.SERIAL_NO FROM TB_H WHERE [NETWORK].[IS_HOST_RESPONSIVE]('RUNNING', TB_H.STATUS) = 'N' -- Shu-Yuan Yang 11142023 modified to use function to determine responsive status
		UNION
		SELECT TB_SER.SERIAL_NO FROM TB_SER WHERE [NETWORK].[IS_HOST_RESPONSIVE](TB_SER.STATUS, 'NORMAL') = 'N'
	)
	INSERT INTO @ProblemHostIP 
	SELECT H.HOST_IP FROM NETWORK.TB_WEBSITE_HOST H WHERE H.SERIAL_NO IN (SELECT TB_PROBLEM_SER.SERIAL_NO FROM TB_PROBLEM_SER);

	UPDATE NETWORK.TB_USER_SESSION SET IS_VALID = 'N'
	WHERE HOST_IP IN (SELECT HOST_IP FROM @ProblemHostIP);

	-- delete from session data and write into session logs:
	EXEC [NETWORK].[INVALIDATE_USER_SESSIONS] @SESSION_ID = 'XXXXXXX', @CLIENT_IP = 'X.X.X.X', @THREAD_ID = 'XXXXXXXX', @HOST_IP = 'X.X.X.X';

END;
