MERGE INTO {TableName} AS TARGET
    USING (VALUES (@ClientId, @Value, @Expiration, @V)) AS SOURCE ([ClientId], [Value], [Expiration], [V])
    ON SOURCE.[ClientId] = TARGET.[ClientId] AND SOURCE.[Value] = TARGET.[Value]
    WHEN MATCHED THEN UPDATE SET [Expiration] = SOURCE.[Expiration], [V] = SOURCE.[V]
    WHEN NOT MATCHED THEN INSERT ([ClientId], [Value], [Expiration], [V]) VALUES (SOURCE.[ClientId], SOURCE.[Value], SOURCE.[Expiration], SOURCE.[V]);
