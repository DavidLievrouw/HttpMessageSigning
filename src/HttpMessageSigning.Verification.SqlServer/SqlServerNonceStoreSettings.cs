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
        public string TableName { get; set; } = "nonces";

        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(ConnectionString)}.");
            if (string.IsNullOrEmpty(TableName)) throw new ValidationException($"The {nameof(SqlServerNonceStoreSettings)} do not specify a valid {nameof(TableName)}.");
        }
    }
}