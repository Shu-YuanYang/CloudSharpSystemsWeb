﻿CREATE TABLE [NETWORK].[TB_USER_SESSION_ITEM] (
    [SESSION_ID]       VARCHAR (100)  NOT NULL,
    [ITEM_NAME]        VARCHAR (250)  NOT NULL,
    [ITEM_DESCRIPTION] NVARCHAR (MAX) NOT NULL,
    [ITEM_SIZE]        INT            NOT NULL,
    [ITEM_ROUTE]       VARCHAR (500)  NOT NULL,
    [ITEM_POLICY]      VARCHAR (500)  NOT NULL,
    [EXPIRATION_TIME]  DATETIME       NULL,
    [EDIT_BY]          VARCHAR (100)  NOT NULL,
    [EDIT_TIME]        DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([SESSION_ID] ASC, [ITEM_NAME] ASC),
    FOREIGN KEY ([SESSION_ID]) REFERENCES [NETWORK].[TB_USER_SESSION] ([SESSION_ID])
);

