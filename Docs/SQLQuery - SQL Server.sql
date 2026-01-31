IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DouTrackerDB')
BEGIN
    CREATE DATABASE DouTrackerDB;
END
GO

USE DouTrackerDB;
GO

-- Ubicaciones (Aca tmb se define la pareja y la sala)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserLocations' AND xtype='U')
BEGIN
    CREATE TABLE UserLocations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId NVARCHAR(450) NOT NULL,
        PairingCode NVARCHAR(50),
        Name NVARCHAR(100),       
        Latitude FLOAT,
        Longitude FLOAT,
        LastUpdate DATETIME2,
        CONSTRAINT UQ_DeviceId UNIQUE(DeviceId) -- Un solo registro por celular
    );
    CREATE INDEX idx_pairing_code ON UserLocations(PairingCode);
END
GO

-- Recomendaciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Recommendations' AND xtype='U')
BEGIN
    CREATE TABLE Recommendations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PairingCode NVARCHAR(50) NOT NULL,  
        TmdbId INT,                         
        Title NVARCHAR(200) NOT NULL,
        MediaType NVARCHAR(20) NOT NULL,    
        ReleaseYear INT,
        Creator NVARCHAR(100),
        CoverUrl NVARCHAR(MAX),
        CurrentStatus NVARCHAR(50) DEFAULT 'Pendiente', -- 'Viendo', 'Terminado' o 'Pendiente'
        AddedByDevice NVARCHAR(450),
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX idx_rec_pairing ON Recommendations(PairingCode);
END
GO

-- Calificar recomendaciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RecommendationRatings' AND xtype='U')
BEGIN
    CREATE TABLE RecommendationRatings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RecommendationId INT FOREIGN KEY REFERENCES Recommendations(Id) ON DELETE CASCADE,
        DeviceId NVARCHAR(450) NOT NULL,
        UserName NVARCHAR(100),
        Score INT CHECK (Score >= 1 AND Score <= 10),
        UNIQUE(RecommendationId, DeviceId) -- Un voto por usuario por peli
    );
END
GO

-- Tablero de mensajes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PostItMessages' AND xtype='U')
BEGIN
    CREATE TABLE PostItMessages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SenderName NVARCHAR(100),     
        TargetDeviceId NVARCHAR(450), 
        Content NVARCHAR(500),       
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    -- Índice para que la búsqueda sea instantánea
    CREATE INDEX idx_postit_target ON PostItMessages(TargetDeviceId);
END
GO