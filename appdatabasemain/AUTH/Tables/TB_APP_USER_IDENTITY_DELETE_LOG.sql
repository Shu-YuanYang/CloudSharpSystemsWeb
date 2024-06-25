﻿CREATE TABLE [AUTH].[TB_APP_USER_IDENTITY_DELETE_LOG] (
    [IDENTITY_PROVIDER] VARCHAR (100) NOT NULL,
    [USERNAME]          VARCHAR (200) NOT NULL,
    [USERID]            VARCHAR (100) NOT NULL,
    [USERNAME_ALIAS]    VARCHAR (200) NOT NULL,
    [LANGUAGE_CODE]     VARCHAR (10)  NOT NULL,
    [IS_ENABLED]        CHAR (1)      NOT NULL,
    [DELETED_BY]        VARCHAR (100) NOT NULL,
    [DELETED_TIME]      DATETIME      DEFAULT (getdate()) NOT NULL,
    CHECK ([IS_ENABLED]='N' OR [IS_ENABLED]='Y')
);

