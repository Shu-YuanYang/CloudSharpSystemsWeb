﻿CREATE TABLE [APPLICATIONS].[TB_PROGRAM_STATUS] (
    [PROGRAM_ID]        VARCHAR (150)  NOT NULL,
    [APP_ID]            VARCHAR (100)  NOT NULL,
    [PROGRAM_TYPE]      VARCHAR (50)   NOT NULL,
    [PROGRAM_STATUS]    VARCHAR (20)   NOT NULL,
    [LAST_TRACE_ID]     VARCHAR (100)  NOT NULL,
    [LAST_LOG_TIME]     DATETIME       NOT NULL,
    [NOTES]             NVARCHAR (500) NOT NULL,
    [MAX_IDLE_INTERVAL] INT            NOT NULL,
    [RESOURCE_SEP]      VARCHAR (100)  NOT NULL,
    [RESOURCE]          VARCHAR (100)  NOT NULL,
    [PROGRAM_PATH]      VARCHAR (2000) NOT NULL,
    [EXECUTION_COMMAND] VARCHAR (2000) NOT NULL,
    [EDIT_BY]           VARCHAR (100)  NOT NULL,
    [EDIT_TIME]         DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([PROGRAM_ID] ASC, [APP_ID] ASC),
    CHECK ([PROGRAM_STATUS]='ERROR' OR [PROGRAM_STATUS]='WARNING' OR [PROGRAM_STATUS]='GOOD'),
    FOREIGN KEY ([APP_ID]) REFERENCES [APPLICATIONS].[TB_APP] ([APP_ID])
);
