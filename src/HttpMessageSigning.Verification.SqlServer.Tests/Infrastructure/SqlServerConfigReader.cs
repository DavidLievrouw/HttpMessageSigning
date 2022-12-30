using System;
using Microsoft.Extensions.Configuration;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure {
    public static class SqlServerConfigReader {
        private const string DefaultConnectionStringTemplate = "Server={SERVER};Database=master;User Id={USERID};Password={PASSWORD};TrustServerCertificate=True;";
        private const string DefaultDatabaseNameTemplate = "HttpMessageSigningTestDb_{0}";
        private const string DefaultServerName = ".";
        private const string DefaultUserId = "sa";
        private const string DefaultPassword = "_777zeven*";

        public static SqlServerConfig Read(IConfiguration config) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var section = config.GetSection("SqlServer");
            var sqlServerConfig = new SqlServerConfig();
            section.Bind(sqlServerConfig);

            if (string.IsNullOrEmpty(sqlServerConfig.ConnectionStringTemplate)) sqlServerConfig.ConnectionStringTemplate = DefaultConnectionStringTemplate;
            if (string.IsNullOrEmpty(sqlServerConfig.DatabaseNameTemplate)) sqlServerConfig.DatabaseNameTemplate = DefaultDatabaseNameTemplate;
            if (string.IsNullOrEmpty(sqlServerConfig.ServerName)) sqlServerConfig.ServerName = DefaultServerName;
            if (string.IsNullOrEmpty(sqlServerConfig.UserId)) sqlServerConfig.UserId = DefaultUserId;
            if (string.IsNullOrEmpty(sqlServerConfig.Password)) sqlServerConfig.Password = DefaultPassword;

            return sqlServerConfig;
        }
    }
}