﻿CREATE TABLE [PRODUCTS].[TB_SERVER] (
    [SERIAL_NO]         VARCHAR (50)   NOT NULL,
    [STATUS]            VARCHAR (20)   NULL,
    [NET_LOAD_CAPACITY] INT            NOT NULL,
    [SERVER_SPEC]       NVARCHAR (300) NOT NULL,
    [CPU]               VARCHAR (100)  NOT NULL,
    [RAM]               VARCHAR (100)  NOT NULL,
    [STORAGE]           VARCHAR (100)  NOT NULL,
    [PSU]               VARCHAR (100)  NOT NULL,
    [FAN]               VARCHAR (100)  NOT NULL,
    [REGISTRATION_DATE] DATE           NOT NULL,
    [LAST_SERVICE_DATE] DATE           NULL,
    [OWNED_BY]          VARCHAR (100)  NOT NULL,
    [LOCATION_CODE]     VARCHAR (20)   NOT NULL,
    [RACK_CODE]         VARCHAR (50)   NOT NULL,
    [EDIT_BY]           VARCHAR (100)  NOT NULL,
    [EDIT_TIME]         DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([SERIAL_NO] ASC),
    CHECK ([STATUS]='REMOVED' OR [STATUS]='IN_REPAIR' OR [STATUS]='OFF' OR [STATUS]='RUNNING')
);

