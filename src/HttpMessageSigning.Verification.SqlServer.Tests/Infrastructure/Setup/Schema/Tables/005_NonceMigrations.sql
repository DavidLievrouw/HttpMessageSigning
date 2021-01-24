﻿SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[NonceMigrations] (
    [Id] [nvarchar](100) NOT NULL,
    [StepName] [nvarchar](100) NOT NULL,
    [Time] [datetimeoffset](3) NOT NULL,
    [MigratorVersion] [nvarchar](20) NOT NULL,
    CONSTRAINT [PK_NonceMigrations] PRIMARY KEY CLUSTERED([Id] ASC)
) 
GO
