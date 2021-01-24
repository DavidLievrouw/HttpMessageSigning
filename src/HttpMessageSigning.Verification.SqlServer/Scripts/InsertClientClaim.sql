INSERT INTO {ClientClaimsTableName} (
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
