INSERT INTO {TableName} (
    [ClientId],
    [Type],
    [Value],
    [Issuer],
    [OriginalIssuer],
    [ValueType]
)
VALUES (
    @ClientId,
    @Type,
    @Value,
    @Issuer,
    @OriginalIssuer,
    @ValueType
)
