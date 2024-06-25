﻿CREATE TABLE [APPLICATIONS].[TB_APP_DATA_CONTROL] (
    [APP_ID]        VARCHAR (100) NOT NULL,
    [CONTROL_NAME]  VARCHAR (50)  NOT NULL,
    [CONTROL_TYPE]  VARCHAR (50)  NOT NULL,
    [CONTROL_LEVEL] VARCHAR (50)  NOT NULL,
    [CONTROL_VALUE] VARCHAR (250) NOT NULL,
    [CONTROL_NOTE]  VARCHAR (500) NOT NULL,
    [IS_ENABLED]    CHAR (1)      NULL,
    [EDIT_BY]       VARCHAR (100) NOT NULL,
    [EDIT_DATE]     DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([APP_ID] ASC, [CONTROL_NAME] ASC, [CONTROL_TYPE] ASC, [CONTROL_LEVEL] ASC, [CONTROL_VALUE] ASC),
    CHECK ([IS_ENABLED]='N' OR [IS_ENABLED]='Y'),
    FOREIGN KEY ([APP_ID]) REFERENCES [APPLICATIONS].[TB_APP] ([APP_ID])
);

