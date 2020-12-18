using System;
using System.IO;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts {
    internal class GenericScriptTemplateReader : IGenericScriptTemplateReader {
        public string ReadCreateEmptyDatabaseScriptTemplate() {
            return ReadGenericScriptTemplate("CreateEmptyDatabase");
        }

        public string ReadDeleteDatabaseScriptTemplate() {
            return ReadGenericScriptTemplate("DeleteDatabase");
        }

        private string ReadGenericScriptTemplate(string genericScriptName) {
            if (string.IsNullOrEmpty(genericScriptName)) throw new ArgumentNullException(nameof(genericScriptName));

            var thisNamespace = GetType().Namespace;
            var embeddedResource = GetType().Assembly.GetManifestResourceStream($"{thisNamespace}.{genericScriptName}.sql");
            if (embeddedResource == null) throw new ArgumentException("The generic script template could not be found.", nameof(genericScriptName));

            using (var streamReader = new StreamReader(embeddedResource)) {
                return streamReader.ReadToEnd();
            }
        }
    }
}