using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts {
    internal class GenericSqlScriptsReader : IGenericSqlScriptsReader {
        private readonly IGenericScriptTemplateReader _genericScriptTemplateReader;

        public GenericSqlScriptsReader(IGenericScriptTemplateReader genericScriptTemplateReader) {
            _genericScriptTemplateReader = genericScriptTemplateReader ?? throw new ArgumentNullException(nameof(genericScriptTemplateReader));
        }

        public string ReadCreateEmptyDatabaseSql(string databaseName) {
            try {
                return string.Format(_genericScriptTemplateReader.ReadCreateEmptyDatabaseScriptTemplate(), databaseName);
            }
            catch (Exception ex) {
                throw new ApplicationException("Cannot access the CreateEmptyDatabase script.", ex);
            }
        }

        public string ReadCloseConnectionsAndDeleteDatabaseSql(string databaseName) {
            try {
                return string.Format(_genericScriptTemplateReader.ReadDeleteDatabaseScriptTemplate(), databaseName);
            }
            catch (Exception ex) {
                throw new ApplicationException("Cannot access the CreateEmptyDatabase script.", ex);
            }
        }
    }
}