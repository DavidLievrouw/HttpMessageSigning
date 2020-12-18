SELECT [ClientId]
    ,[Value]
    ,[Expiration]
FROM {TableName}
WHERE
    [ClientId] = @ClientId AND
    [Value] = @Value