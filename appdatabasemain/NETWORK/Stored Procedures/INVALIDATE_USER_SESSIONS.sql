﻿
CREATE PROCEDURE [NETWORK].[INVALIDATE_USER_SESSIONS]
	@SESSION_ID AS VARCHAR(100) = NULL,
	@CLIENT_IP AS VARCHAR(20) = NULL,
	@THREAD_ID AS VARCHAR(100) = NULL,
	@HOST_IP AS VARCHAR(20) = NULL
	-- Created by Shu-Yuan Yang 05/15/2024
	-- Invalidate sessions and clear the session data.
AS
BEGIN
	
	--SELECT * FROM NETWORK.TB_USER_SESSION
	UPDATE NETWORK.TB_USER_SESSION SET IS_VALID = 'N'
	WHERE (ISNULL(@SESSION_ID, '') = '' OR [SESSION_ID] = @SESSION_ID)
	AND (ISNULL(@CLIENT_IP, '') = '' OR CLIENT_IP = @CLIENT_IP)
	AND (ISNULL(@THREAD_ID, '') = '' OR THREAD_ID = @THREAD_ID)
	AND (ISNULL(@HOST_IP, '') = '' OR HOST_IP = @HOST_IP);

	DECLARE @INVALIDATED_SESSIONS TABLE([SESSION_ID] VARCHAR(100));

	INSERT INTO @INVALIDATED_SESSIONS
	SELECT [SESSION_ID] FROM NETWORK.TB_USER_SESSION
	WHERE IS_VALID = 'N';

	INSERT INTO NETWORK.TB_USER_SESSION_HISTORY_LOG
	SELECT TB_S.[SESSION_ID], TB_S.CLIENT_IP, TB_S.THREAD_ID, TB_S.HOST_IP, TB_S.RESOURCE_UNIT, TB_S.CLIENT_LOCATION, TB_S.REQUESTED_TIME, TB_S.RESOURCE_SIZE, TB_S.EDIT_BY, TB_S.EDIT_TIME, GETDATE()
	FROM NETWORK.TB_USER_SESSION TB_S WHERE TB_S.[SESSION_ID] IN (SELECT [SESSION_ID] FROM @INVALIDATED_SESSIONS);

	INSERT INTO NETWORK.TB_USER_SESSION_ITEM_HISTORY_LOG
	SELECT TB_I.*
	FROM NETWORK.TB_USER_SESSION_ITEM TB_I WHERE TB_I.[SESSION_ID] IN (SELECT [SESSION_ID] FROM @INVALIDATED_SESSIONS);
	
	-- SELECT * FROM NETWORK.TB_USER_SESSION_ITEM
	DELETE FROM NETWORK.TB_USER_SESSION_ITEM
	WHERE [SESSION_ID] IN (SELECT [SESSION_ID] FROM @INVALIDATED_SESSIONS);

	-- SELECT * FROM NETWORK.TB_USER_SESSION
	DELETE FROM NETWORK.TB_USER_SESSION
	WHERE [SESSION_ID] IN (SELECT [SESSION_ID] FROM @INVALIDATED_SESSIONS);

END;