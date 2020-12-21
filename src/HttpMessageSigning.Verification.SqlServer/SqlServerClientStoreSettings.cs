using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    /// <summary>
    ///     Represents settings for SQL Server storage of registered <see cref="Client" /> instances.
    /// </summary>
    public class SqlServerClientStoreSettings {
        /// <summary>
        ///     Gets or sets the connection string to the SQL Server database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the name of the SQL Server table that will contain registered clients.
        /// </summary>
        /// <remarks>When the table is not in the default schema, you can prepend the schema name to this value.</remarks>
        public string ClientsTableName { get; set; } = "Clients";
        
        /// <summary>
        ///     Gets or sets the name of the SQL Server table that will contain the assigned claims for the registered clients.
        /// </summary>
        /// <remarks>When the table is not in the default schema, you can prepend the schema name to this value.</remarks>
        public string ClientClaimsTableName { get; set; } = "ClientClaims";
        
        /// <summary>
        ///     Gets or sets the name of the SQL Server table that will contain the schema version info.
        /// </summary>
        /// <remarks>When the table is not in the default schema, you can prepend the schema name to this value.</remarks>
        public string VersionTableName { get; set; } = "ClientVersions";

        /// <summary>
        ///     Gets or sets the encryption key for the shared secrets.
        /// </summary>
        /// <remarks>This only applies to signature algorithms that use symmetric keys, e.g. HMAC. Set this value to <see langword="null" /> to disable encryption.</remarks>
        public SharedSecretEncryptionKey SharedSecretEncryptionKey { get; set; } = SharedSecretEncryptionKey.Empty;

        /// <summary>
        ///     Gets or sets the time that client queries are cached in memory.
        /// </summary>
        /// <remarks>Set to <see cref="TimeSpan.Zero" /> to disable caching.</remarks>
        public TimeSpan ClientCacheEntryExpiration { get; set; } = TimeSpan.Zero;

        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ValidationException($"The {nameof(SqlServerClientStoreSettings)} do not specify a valid {nameof(ConnectionString)}.");
            if (string.IsNullOrEmpty(ClientsTableName)) throw new ValidationException($"The {nameof(SqlServerClientStoreSettings)} do not specify a valid {nameof(ClientsTableName)}.");
            if (string.IsNullOrEmpty(ClientClaimsTableName)) throw new ValidationException($"The {nameof(SqlServerClientStoreSettings)} do not specify a valid {nameof(ClientClaimsTableName)}.");
            if (string.IsNullOrEmpty(VersionTableName)) throw new ValidationException($"The {nameof(SqlServerClientStoreSettings)} do not specify a valid {nameof(VersionTableName)}.");
        }
    }
}