USE [PingMe];
GO

DROP INDEX [IX_Source_Lookup] ON [Transaction].[Messages];
GO

DROP TABLE [Transaction].[Messages];
GO

DROP SCHEMA [Transaction];
GO

/*DROP DATABASE [PingMe];
GO*/