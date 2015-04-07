USE [PingMe];
GO

CREATE SCHEMA [Transaction];
GO

CREATE TABLE [Transaction].[Messages]
(
	Id [uniqueidentifier] NOT NULL,
	Source [char](15) NOT NULL,
	Destination [char](15) NOT NULL,
	Message [nvarchar](140) NULL,
	Asset [varbinary](max) NULL
) ON [PRIMARY];
GO

ALTER TABLE [Transaction].[Messages]
ADD CONSTRAINT Message_Identifier PRIMARY KEY CLUSTERED
(
	Id
)
WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];

CREATE NONCLUSTERED INDEX IX_Source_Lookup ON [Transaction].[Messages]
(
	Source
)
WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]; 
GO

ALTER TABLE [Transaction].[Messages]
ADD	MessageSentUTC [datetime] NULL;
GO

ALTER TABLE [Transaction].[Messages]
ADD MessageRecievedUTC [datetime] NULL;
GO

ALTER TABLE [Transaction].[Messages] WITH NOCHECK
ADD CONSTRAINT [ValidateDateRange] CHECK ([MessageRecievedUTC] > [MessageSentUTC]);
GO