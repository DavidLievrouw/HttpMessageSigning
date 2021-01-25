SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Clients] (
    [Id] [nvarchar](100) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [NonceLifetime] [decimal](5, 0) NOT NULL,
    [ClockSkew] [decimal](5, 0) NOT NULL,
    [SigType] [nvarchar](20) NOT NULL,
    [SigParameter] [nvarchar](max) NOT NULL,
    [SigHashAlgorithm] [nvarchar](20) NOT NULL,
    [IsSigParameterEncrypted] [bit] NOT NULL,
    [RequestTargetEscaping] [nvarchar](20) NOT NULL,
    [V] [tinyint] NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE TABLE [dbo].[ClientClaims](
    [ClientId] [nvarchar](100) NOT NULL,
    [Type] [nvarchar](100) NOT NULL,
    [Value] [nvarchar](100) NOT NULL,
    [Issuer] [nvarchar](100) NOT NULL,
    [OriginalIssuer] [nvarchar](100) NOT NULL,
    [ValueType] [nvarchar](100) NOT NULL
)
GO

ALTER TABLE [dbo].[ClientClaims] WITH CHECK ADD CONSTRAINT [FK_ClientClaims_Clients] FOREIGN KEY([ClientId])
    REFERENCES [dbo].[Clients] ([Id])
    ON UPDATE CASCADE
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClientClaims] CHECK CONSTRAINT [FK_ClientClaims_Clients]
GO

CREATE TABLE [dbo].[ClientMigrations] (
    [Id] [nvarchar](100) NOT NULL,
    [StepName] [nvarchar](100) NOT NULL,
    [Time] [datetimeoffset](3) NOT NULL,
    [MigratorVersion] [nvarchar](20) NOT NULL,
    CONSTRAINT [PK_ClientMigrations] PRIMARY KEY CLUSTERED([Id] ASC)
)
GO

CREATE TABLE [dbo].[Nonces] (
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [ClientId] [nvarchar](100) NOT NULL,
    [Value] [nvarchar](36) NOT NULL,
    [Expiration] [datetimeoffset](3) NOT NULL,
    [V] [tinyint] NOT NULL,
    CONSTRAINT [PK_Nonces] PRIMARY KEY CLUSTERED([Id] ASC)
)
GO

IF EXISTS (SELECT [name] FROM sys.indexes WHERE [name] = N'IX_Expiration')
    DROP INDEX IX_Expiration ON [dbo].[Nonces];
GO

CREATE NONCLUSTERED INDEX IX_Expiration
    ON [dbo].[Nonces] ([Expiration]);

GO

IF EXISTS (SELECT [name] FROM sys.indexes WHERE [name] = N'IX_Client_Value')
    DROP INDEX IX_Client_Value ON [dbo].[Nonces];
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Client_Value
    ON [dbo].[Nonces] ([ClientId], [Value]);

GO

CREATE TABLE [dbo].[NonceMigrations] (
    [Id] [nvarchar](100) NOT NULL,
    [StepName] [nvarchar](100) NOT NULL,
    [Time] [datetimeoffset](3) NOT NULL,
    [MigratorVersion] [nvarchar](20) NOT NULL,
    CONSTRAINT [PK_NonceMigrations] PRIMARY KEY CLUSTERED([Id] ASC)
)
GO
