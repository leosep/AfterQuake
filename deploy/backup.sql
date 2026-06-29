-- AfterQuake Database Backup Script
-- Programar en SQL Agent para ejecutar cada 6 horas

DECLARE @BackupPath NVARCHAR(500) = 'C:\Backups\AfterQuake\'
DECLARE @FileName NVARCHAR(500) = @BackupPath + 'AfterQuake_Full_' + 
    REPLACE(CONVERT(NVARCHAR, GETDATE(), 120), ':', '-') + '.bak'

-- Asegurar que el directorio existe
EXEC master.dbo.xp_create_subdir @BackupPath

-- Backup full
BACKUP DATABASE [AfterQuake]
TO DISK = @FileName
WITH INIT, 
     NAME = N'AfterQuake-Full Database Backup',
     COMPRESSION,
     STATS = 10

-- Backup de log
BACKUP LOG [AfterQuake]
TO DISK = @BackupPath + 'AfterQuake_Log_' + 
    REPLACE(CONVERT(NVARCHAR, GETDATE(), 120), ':', '-') + '.trn'
WITH INIT, COMPRESSION

-- Limpiar backups mayores a 30 dÃ­as
DECLARE @DeleteDate NVARCHAR(20) = CONVERT(NVARCHAR, DATEADD(DAY, -30, GETDATE()), 112)
EXEC master.dbo.xp_delete_file 0, @BackupPath, N'bak', @DeleteDate, 1
EXEC master.dbo.xp_delete_file 0, @BackupPath, N'trn', @DeleteDate, 1
