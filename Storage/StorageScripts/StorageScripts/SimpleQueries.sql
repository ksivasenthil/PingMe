USE [PingMe];
GO
SELECT M.*
FROM
dbo.Messages M
WHERE
M.Source = '+919840200524'
AND
M.Destination = '+919941841903';

SELECT M.*
FROM
dbo.Messages M
WHERE
M.Destination = '+919840200524'
AND
M.Source = '+919941841903';