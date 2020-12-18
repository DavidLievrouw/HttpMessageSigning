IF NOT EXISTS(SELECT principal_id FROM sys.sql_logins WHERE name = 'dalion') BEGIN
    CREATE LOGIN dalion 
    WITH PASSWORD = 'S3cr37_P@$$w0rD';
END;
GO

IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'dalion') BEGIN
    CREATE USER dalion
    FOR LOGIN dalion;
END;
GO

EXEC sp_addrolemember N'db_datareader', N'dalion';
GO 

EXEC sp_addrolemember N'db_datawriter', N'dalion';
GO
