-- Plan académico sencillo:
-- 1) Modelo de recuperación FULL.
ALTER DATABASE GranOriente SET RECOVERY FULL;
GO

-- 2) Ejemplo de backup completo diario (ajustar ruta).
-- BACKUP DATABASE GranOriente
-- TO DISK='C:\Backups\GranOriente_FULL.bak'
-- WITH INIT, COMPRESSION, CHECKSUM;

-- 3) Ejemplo de backup diferencial cada 6 horas.
-- BACKUP DATABASE GranOriente
-- TO DISK='C:\Backups\GranOriente_DIFF.bak'
-- WITH DIFFERENTIAL, COMPRESSION, CHECKSUM;

-- 4) Ejemplo de log cada 30 minutos.
-- BACKUP LOG GranOriente
-- TO DISK='C:\Backups\GranOriente_LOG.trn'
-- WITH COMPRESSION, CHECKSUM;
