﻿

CREATE PROCEDURE [INTERFACES].[ADD_WEBSITE_MENU_ITEM]
	@MENU_HEADER_ID VARCHAR(100),
	@ITEM_NAME VARCHAR(100),
	@ITEM_DISPLAY_NAME VARCHAR(100),
	@ROUTE_TYPE VARCHAR(50),
	@ROUTE VARCHAR(500),
	@ICON VARCHAR(200),
	@EDIT_BY VARCHAR(100)
AS BEGIN
	
	DECLARE @is_enabled CHAR(1);

	-- Validate item name:
	SELECT TOP 1 @is_enabled = IS_ENABLED FROM INTERFACES.TB_WEBSITE_MENU_ITEM
	WHERE HEADER_ID = @MENU_HEADER_ID AND ITEM_NAME = @ITEM_NAME;
	-- AND IS_ENABLED = 'Y';

	SET @is_enabled = ISNULL(@is_enabled, '');
	IF @is_enabled = 'Y' THROW 51002, 'Item ID is already taken!', 1;

	-- Validate route:
	EXEC INTERFACES.VALIDATE_WEBSITE_MENU_ROUTE @ROUTE_TYPE=@ROUTE_TYPE, @ROUTE=@ROUTE;

	-- Compute ranking: 
	DECLARE @NEW_ITEM_RANK INT = (SELECT ISNULL(MAX(RANKING), 0) FROM INTERFACES.TB_WEBSITE_MENU_ITEM WHERE HEADER_ID = @MENU_HEADER_ID);
	IF @NEW_ITEM_RANK < 0 SET @NEW_ITEM_RANK = 0; -- Set as active item if current menu only contains inactive (RANKING = -1) ones.

	IF @is_enabled = 'N'
		UPDATE INTERFACES.TB_WEBSITE_MENU_ITEM 
		SET
		DISPLAY_NAME = @ITEM_DISPLAY_NAME,
		ROUTE_TYPE = @ROUTE_TYPE,
		[ROUTE] = @ROUTE,
		ICON = @ICON,
		RANKING = @NEW_ITEM_RANK + 1,
		IS_ENABLED = 'Y',
		EDIT_BY = @EDIT_BY,
		EDIT_TIME = GETDATE()
		WHERE HEADER_ID = @MENU_HEADER_ID AND ITEM_NAME = @ITEM_NAME;
	ELSE
		INSERT INTO INTERFACES.TB_WEBSITE_MENU_ITEM 
		SELECT 
		@MENU_HEADER_ID AS HEADER_ID,
		@ITEM_NAME AS ITEM_NAME, 
		@ITEM_DISPLAY_NAME AS DISPLAY_NAME,
		@ROUTE_TYPE AS ROUTE_TYPE,
		@ROUTE AS [ROUTE],
		@ICON AS ICON,
		@NEW_ITEM_RANK + 1 AS RANKING,
		'Y' AS IS_ENABLED, 
		@EDIT_BY AS EDIT_BY,
		GETDATE() AS EDIT_TIME;

END;