﻿CREATE TABLE [AUTH].[TB_APP_TEAM] (
    [TEAM_ID]          VARCHAR (100)   DEFAULT ((format(getdate(),'yyyyMMddHHmmss')+'_')+CONVERT([nvarchar](50),newid())) NOT NULL,
    [APP_ID]           VARCHAR (100)   NULL,
    [TEAM_NAME]        NVARCHAR (50)   NOT NULL,
    [TEAM_DESCRIPTION] NVARCHAR (1000) NOT NULL,
    [PROFILE_PICTURE]  VARCHAR (500)   NOT NULL,
    [PRIMARY_CONTACT]  VARCHAR (200)   NOT NULL,
    [IS_ENABLED]       CHAR (1)        NOT NULL,
    [OWNED_BY]         VARCHAR (100)   NOT NULL,
    [EDIT_BY]          VARCHAR (100)   NOT NULL,
    [EDIT_TIME]        DATETIME        DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([TEAM_ID] ASC),
    CHECK ([IS_ENABLED]='N' OR [IS_ENABLED]='Y'),
    FOREIGN KEY ([APP_ID]) REFERENCES [APPLICATIONS].[TB_APP] ([APP_ID]),
    CONSTRAINT [UQ_TB_APP_T_APPTEAM] UNIQUE NONCLUSTERED ([APP_ID] ASC, [TEAM_NAME] ASC)
);



