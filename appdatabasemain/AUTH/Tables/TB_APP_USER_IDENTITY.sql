﻿CREATE TABLE [AUTH].[TB_APP_USER_IDENTITY] (
    [IDENTITY_PROVIDER] VARCHAR (100) NOT NULL,
    [USERNAME]          VARCHAR (200) NOT NULL,
    [USERID]            VARCHAR (100) NULL,
    [USERNAME_ALIAS]    VARCHAR (200) NOT NULL,
    [LANGUAGE_CODE]     VARCHAR (10)  NOT NULL,
    [IS_ENABLED]        CHAR (1)      NOT NULL,
    [EDIT_BY]           VARCHAR (100) NOT NULL,
    [EDIT_TIME]         DATETIME      DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([IDENTITY_PROVIDER] ASC, [USERNAME] ASC),
    CHECK ([IS_ENABLED]='N' OR [IS_ENABLED]='Y'),
    FOREIGN KEY ([USERID]) REFERENCES [AUTH].[TB_APP_USER] ([USERID]),
    CONSTRAINT [AUTH_APP_USER_IDENTITY_ALIAS] UNIQUE NONCLUSTERED ([IDENTITY_PROVIDER] ASC, [USERNAME_ALIAS] ASC)
);
