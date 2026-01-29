IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DouTrackerDB')
BEGIN
    CREATE DATABASE DouTrackerDB;
    PRINT 'Base de datos DouTrackerDB creada.';
END
GO

USE DouTrackerDB;
GO

-- 1. TABLA DE UBICACIONES (Aquí guardamos quién es quién)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserLocations' AND xtype='U')
BEGIN
    CREATE TABLE UserLocations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId NVARCHAR(450) NOT NULL,
        PairingCode NVARCHAR(50), -- Código de la sala
        Name NVARCHAR(100),       -- Nombre del usuario
        Latitude FLOAT,
        Longitude FLOAT,
        LastUpdate DATETIME2,
        CONSTRAINT UQ_DeviceId UNIQUE(DeviceId) -- Un solo registro por celular
    );
    CREATE INDEX idx_pairing_code ON UserLocations(PairingCode);
    PRINT 'Tabla UserLocations creada.';
END
GO

-- 2. TABLA DE RECOMENDACIONES (MediaHub)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Recommendations' AND xtype='U')
BEGIN
    CREATE TABLE Recommendations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PairingCode NVARCHAR(50) NOT NULL,  -- Vincula con la sala
        TmdbId INT,                         -- ID de la película/serie en API externa
        Title NVARCHAR(200) NOT NULL,
        MediaType NVARCHAR(20) NOT NULL,    -- 'Pelicula', 'Serie'
        ReleaseYear INT,
        Creator NVARCHAR(100),
        CoverUrl NVARCHAR(MAX),
        CurrentStatus NVARCHAR(50) DEFAULT 'Pendiente', -- 'Viendo', 'Terminado'
        AddedByDevice NVARCHAR(450),
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX idx_rec_pairing ON Recommendations(PairingCode);
    PRINT 'Tabla Recommendations creada.';
END
GO

-- 3. TABLA DE VOTOS (Para calificar recomendaciones)
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
    PRINT 'Tabla RecommendationRatings creada.';
END
GO

-- 4. TABLA DE MENSAJES (SISTEMA POST-IT / POLLING)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PostItMessages' AND xtype='U')
BEGIN
    CREATE TABLE PostItMessages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SenderName NVARCHAR(100),     -- Quién lo manda
        TargetDeviceId NVARCHAR(450), -- Para quién es (Su DeviceId)
        Content NVARCHAR(500),        -- El mensaje
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    -- Índice para que la búsqueda sea instantánea
    CREATE INDEX idx_postit_target ON PostItMessages(TargetDeviceId);
    PRINT 'Tabla PostItMessages creada.';
END
GO