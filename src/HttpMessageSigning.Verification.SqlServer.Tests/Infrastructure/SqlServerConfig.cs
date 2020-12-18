using Microsoft.Data.SqlClient;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure {
    public class SqlServerConfig {
        private const string ServerToken = "{SERVER}";
        private const string PasswordToken = "{PASSWORD}";
        private const string UserIdToken = "{USERID}";

        private UniqueDbName _uniqueDbName;

        public string ConnectionStringTemplate { get; set; }
        public string ServerName { get; set; }
        public string DatabaseNameTemplate { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        public string GetConnectionStringForMasterDatabase() {
            return ConnectionStringTemplate
                .Replace(ServerToken, ServerName)
                .Replace(UserIdToken, UserId)
                .Replace(PasswordToken, Password);
        }

        public string GetConnectionStringForTestDatabase() {
            return new SqlConnectionStringBuilder(GetConnectionStringForMasterDatabase()) {
                InitialCatalog = GetUniqueDatabaseName()
            }.ConnectionString;
        }

        public string GetUniqueDatabaseName() {
            if (_uniqueDbName == null) _uniqueDbName = new UniqueDbName(DatabaseNameTemplate, IsDebugMode());
            return _uniqueDbName.ToString();
        }

        private static bool IsDebugMode() {
            var isDebugMode = false;
#if DEBUG
            isDebugMode = true;
#endif
            return isDebugMode;
        }
    }
}