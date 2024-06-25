﻿CREATE PROCEDURE INTERFACES.VALIDATE_WEBSITE_MENU_ROUTE @ROUTE_TYPE VARCHAR(50), @ROUTE VARCHAR(500) AS
BEGIN

	--DECLARE @ROUTE_TYPE VARCHAR(50) = 'ASSET';
	--DECLARE @ROUTE VARCHAR(MAX) = 'https://docs.cosmos.network/v0.46/run-node/run-testnet.html';
	--DECLARE @ROUTE VARCHAR(MAX) = '/charts/mini-monitor';
	DECLARE @VALID CHAR(1);
	
	--DECLARE @VALIDATOR NVARCHAR(MAX) = '(CASE WHEN (@ROUTE LIKE ''http://%'' OR @ROUTE LIKE ''https://%'') THEN ''Y'' ELSE ''N'' END)';
	--DECLARE @VALIDATOR NVARCHAR(MAX) = '(CASE WHEN @ROUTE LIKE ''/%'' THEN ''Y'' ELSE ''N'' END)';
	DECLARE @VALIDATOR NVARCHAR(MAX);
	SELECT TOP 1 @VALIDATOR = CONTROL_VALUE FROM APPLICATIONS.TB_APP_DATA_CONTROL
	WHERE APP_ID = 'CloudSharpVisualDashboard' AND CONTROL_NAME = 'WEBSITE_MENU_CONFIG' 
	AND CONTROL_TYPE = 'ROUTE_VALIDATION_FUNC' AND CONTROL_LEVEL = @ROUTE_TYPE
	AND IS_ENABLED = 'Y';

	IF ISNULL(@VALIDATOR, '') = '' THROW 51000, 'Path type not recognized!', 1;
	

	DECLARE @SQL NVARCHAR(MAX) = N'SELECT @VALIDOUT = ' + @VALIDATOR + ';';
	EXECUTE sp_executesql @SQL, N'@ROUTE VARCHAR(500), @VALIDOUT CHAR(1) OUTPUT', @ROUTE=@ROUTE, @VALIDOUT=@VALID OUTPUT;

	IF @VALID <> 'Y' THROW 51001, 'Invalid path!', 1;
END;