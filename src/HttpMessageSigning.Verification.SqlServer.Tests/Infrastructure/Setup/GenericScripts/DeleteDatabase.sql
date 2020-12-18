USE [master]
GO

IF EXISTS(select * from sys.databases where name='{0}')
  ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

IF EXISTS(select * from sys.databases where name='{0}')
  DROP DATABASE [{0}];
GO