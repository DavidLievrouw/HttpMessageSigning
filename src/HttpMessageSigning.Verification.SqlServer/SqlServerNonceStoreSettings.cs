using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    /// <summary>
    ///     Represents settings for SQL Server storage of registered <see cref="Nonce" /> instances.
    /// </summary>
    public class SqlServerNonceStoreSettings {
        /// <summary>
        ///     Gets or sets the connection string to the SQL Server database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the name of the SQL Server table that will contain nonces.
        /// </summary>
        /// <remarks>When the table is not in the default schema, you can prepend the schema name to this value.</remarks>
        public string NonceTableName { get; set; } = "Nonces";

        /// <summary>
        ///     Gets or sets the name of the SQL Server table that will contain the schema version info.
        /// </summary>
        /// <remarks>When the table is not in the default schema, you can prepend the schema name to this value.</remarks>
        public string VersionsTableName { get; set; } = "NonceVersions";
        
        /// <summary>
        ///     Gets or sets the minimum interval between expired <see cref="Nonce" /> clean-up background task executions.
        /// </summary>
        public TimeSpan ExpiredNoncesCleanUpInterval { get; set; } = TimeSpan.FromMinutes(1);

        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) {
                throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(ConnectionString)}.");
            }
            if (ExpiredNoncesCleanUpInterval <= TimeSpan.Zero) {
                throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(ExpiredNoncesCleanUpInterval)}.");
            }
            if (string.IsNullOrEmpty(NonceTableName)) {
                throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(NonceTableName)}.");
            }
            if (string.IsNullOrEmpty(VersionsTableName)) {
                throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(VersionsTableName)}.");
            }

            if (string.IsNullOrEmpty(NonceTableName)) throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(NonceTableName)}.");
        }
    }
}