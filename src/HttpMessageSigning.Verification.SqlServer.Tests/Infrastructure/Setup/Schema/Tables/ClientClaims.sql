SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
