SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Game](
    [Game] [int] IDENTITY(1,1) NOT NULL,
    [HomeTeam] [int] NOT NULL,
    [AwayTeam] [int] NOT NULL,
    [GoalsHome] [int] NOT NULL,
    [GoalsAway] [int] NOT NULL,
    CONSTRAINT [PK_Game] PRIMARY KEY CLUSTERED ([Game] ASC)
)
GO
