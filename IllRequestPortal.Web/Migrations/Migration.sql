/*
truncate table dbo.IllRequest 
drop table dbo.IllRequest
*/

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IllRequest')
BEGIN
  CREATE TABLE dbo.IllRequest
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    Title NVARCHAR(500) NULL,
    Author NVARCHAR(255) NULL,
    ArticleTitle NVARCHAR(500) NULL,
    ArticleAuthor NVARCHAR(255) NULL,

    PublicationYear NVARCHAR(50) NULL,
    Edition NVARCHAR(100) NULL,
    Isbn NVARCHAR(50) NULL,
    Issn NVARCHAR(50) NULL,
    Volume NVARCHAR(50) NULL,
    Issue NVARCHAR(50) NULL,
    Pages NVARCHAR(50) NULL,

    MaterialType NVARCHAR(50) NOT NULL,

    RequesterName NVARCHAR(255) NOT NULL,
    RequesterEmail NVARCHAR(255) NOT NULL,
    CardNumber NVARCHAR(100) NULL,

    Status NVARCHAR(100) NOT NULL,
    CreatedOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedOn DATETIME2 NULL,
    AddedInLibrisOn DATETIME2 NULL
);
END
GO


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Log')
BEGIN
    CREATE TABLE dbo.Log
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

        Origin NVARCHAR(255) NULL,
        Message NVARCHAR(MAX) NULL,
        LogLevel NVARCHAR(50) NULL,
        Exception NVARCHAR(MAX) NULL,
        Trace NVARCHAR(MAX) NULL,

        CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_Log_CreatedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Migration')
BEGIN
    CREATE TABLE dbo.Migration
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClientVersion NVARCHAR(50) NOT NULL,
        DatabaseVersion NVARCHAR(50) NOT NULL,
        AppliedOn DATETIME2 NOT NULL CONSTRAINT DF_Migration_AppliedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_IllRequest_Status'
      AND object_id = OBJECT_ID('dbo.IllRequest')
)
BEGIN
    CREATE INDEX IX_IllRequest_Status
        ON dbo.IllRequest(Status);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_IllRequest_CardNumber'
      AND object_id = OBJECT_ID('dbo.IllRequest')
)
BEGIN
    CREATE INDEX IX_IllRequest_CardNumber
        ON dbo.IllRequest(CardNumber);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_IllRequest_CreatedOn'
      AND object_id = OBJECT_ID('dbo.IllRequest')
)
BEGIN
    CREATE INDEX IX_IllRequest_CreatedOn
        ON dbo.IllRequest(CreatedOn);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Log_CreatedOn'
      AND object_id = OBJECT_ID('dbo.Log')
)
BEGIN
    CREATE INDEX IX_Log_CreatedOn
        ON dbo.Log(CreatedOn);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE name = 'Description'
      AND object_id = OBJECT_ID('dbo.IllRequest')
)
BEGIN
    ALTER TABLE dbo.IllRequest
        ADD Description NVARCHAR(MAX) NULL;
END
GO