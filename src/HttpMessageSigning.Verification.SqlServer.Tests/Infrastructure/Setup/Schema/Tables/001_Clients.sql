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
