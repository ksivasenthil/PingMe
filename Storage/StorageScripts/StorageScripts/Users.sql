USE [PingMe];
GO

CREATE LOGIN [Pinger] 
	WITH PASSWORD = 'S@^en%u#3';
GO

CREATE USER [Pinger] 
	FOR LOGIN [Pinger];
GO

GRANT SELECT ON SCHEMA::[dbo]
TO [Pinger];
GO

GRANT INSERT ON SCHEMA::[dbo]
TO [Pinger];
GO

GRANT UPDATE ON SCHEMA::[dbo]
TO [Pinger];
GO