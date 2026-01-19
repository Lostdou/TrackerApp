-- 1. CREAR LA BASE DE DATOS (Solo si no existe)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DouTrackerDB')
BEGIN
    CREATE DATABASE DouTrackerDB;
    PRINT 'Base de datos DouTrackerDB creada.';
END
GO

USE DouTrackerDB;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserLocations' AND xtype='U')
BEGIN
    CREATE TABLE UserLocations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId NVARCHAR(450) NOT NULL,
        PairingCode NVARCHAR(50),
        Name NVARCHAR(100),
        Latitude FLOAT,
        Longitude FLOAT,
        LastUpdate DATETIME2
    );
    PRINT 'Tabla UserLocations creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='UQ_DeviceId' AND object_id = OBJECT_ID('UserLocations'))
BEGIN
    ALTER TABLE UserLocations ADD CONSTRAINT UQ_DeviceId UNIQUE(DeviceId);
    PRINT 'Constraint UNIQUE agregada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='idx_pairing_code' AND object_id = OBJECT_ID('UserLocations'))
BEGIN
    CREATE INDEX idx_pairing_code ON UserLocations(PairingCode);
    PRINT 'Índice creado.';
END
GO

---- 6. DATOS DE PRUEBA
--MERGE INTO UserLocations AS target
--USING (SELECT 'TEST_DEVICE_LOCAL' AS DeviceId) AS source
--ON (target.DeviceId = source.DeviceId)
--WHEN NOT MATCHED THEN
--    INSERT (DeviceId, PairingCode, Name, Latitude, Longitude, LastUpdate)
--    VALUES ('TEST_DEVICE_LOCAL', 'TEST-LOCAL', 'Usuario Local', -34.6037, -58.3816, SYSUTCDATETIME());

PRINT '--- SETUP FINALIZADO CORRECTAMENTE ---';