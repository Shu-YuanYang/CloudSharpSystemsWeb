CREATE TABLE [NETWORK].[TB_WEBSITE] (
    [SITE_ID]                  VARCHAR (100) NOT NULL,
    [APP_ID]                   VARCHAR (100) NULL,
    [DOMAIN_NAME]              VARCHAR (300) NOT NULL,
    [SITE_NAME]                VARCHAR (100) NOT NULL,
    [LOAD_BALANCING_ALGORITHM] VARCHAR (50)  NOT NULL,
    [THEME_COLOR]              VARCHAR (20)  NOT NULL,
    [IS_ENABLED]               CHAR (1)      NULL,
    [OWNED_BY]                 VARCHAR (100) NOT NULL,
    [EDIT_BY]                  VARCHAR (100) NOT NULL,
    [EDIT_TIME]                DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([SITE_ID] ASC),
    CHECK ([IS_ENABLED]='N' OR [IS_ENABLED]='Y'),
    FOREIGN KEY ([APP_ID]) REFERENCES [APPLICATIONS].[TB_APP] ([APP_ID])
);

