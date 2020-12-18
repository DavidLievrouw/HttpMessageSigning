MERGE INTO {TableName} AS TARGET
    USING (VALUES (@ClientId, @Value, @Expiration)) AS SOURCE ([ClientId], [Value], [Expiration])
    ON SOURCE.[ClientId] = TARGET.[ClientId] AND SOURCE.[Value] = TARGET.[Value]
    WHEN MATCHED THEN UPDATE SET [Expiration] = SOURCE.[Expiration]
    WHEN NOT MATCHED THEN INSERT ([ClientId], [Value], [Expiration]) VALUES (SOURCE.[ClientId], SOURCE.[Value], SOURCE.[Expiration]);
