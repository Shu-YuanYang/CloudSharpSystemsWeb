﻿CREATE TABLE [AUTH].[TB_APP_USER_TEAM] (
    [USERID]     VARCHAR (100) NOT NULL,
    [TEAM_ID]    VARCHAR (100) NOT NULL,
    [IS_ENABLED] CHAR (1)      NOT NULL,
    [EDIT_BY]    VARCHAR (100) NOT NULL,
    [EDIT_TIME]  DATETIME      DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([USERID] ASC, [TEAM_ID] ASC),
    CHECK ([IS_ENABLED]='N' OR [IS_ENABLED]='Y'),
    FOREIGN KEY ([TEAM_ID]) REFERENCES [AUTH].[TB_APP_TEAM] ([TEAM_ID]),
    FOREIGN KEY ([USERID]) REFERENCES [AUTH].[TB_APP_USER] ([USERID])
);
