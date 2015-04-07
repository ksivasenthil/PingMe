USE [PingMe];
GO

REVOKE SELECT ON SCHEMA::[dbo]
TO [Pinger];
GO

REVOKE INSERT ON SCHEMA::[dbo]
TO [Pinger];
GO

REVOKE UPDATE ON SCHEMA::[dbo]
TO [Pinger];
GO

DROP USER [Pinger];
GO

DROP LOGIN [Pinger];
GO