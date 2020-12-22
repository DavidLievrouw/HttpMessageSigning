SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
