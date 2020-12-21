SELECT [ClientId]
    ,[Value]
    ,[Expiration]
    ,[V]
FROM {TableName}
WHERE
    [ClientId] = @ClientId AND
    [Value] = @Value