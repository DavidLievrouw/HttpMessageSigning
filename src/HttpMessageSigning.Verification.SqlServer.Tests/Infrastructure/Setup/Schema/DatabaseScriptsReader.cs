using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.Schema {
    public class DatabaseScriptsReader : IDatabaseScriptsReader {
        private static readonly string[] FolderPatterns = {
            "Schemas",
            "Types",
            "Tables",
            "Security"
        };

        private static readonly ISet<string> SqlExtensions = new HashSet<string> {
            ".sql",
            ".prc",
            ".viw"
        };

        public IEnumerable<string> ReadAllDatabaseScriptsInOrder() {
            return from folderPattern in FolderPatterns
                from sql in ReadAllSqlFiles(folderPattern)
                select sql;
        }

        private IEnumerable<string> ReadAllSqlFiles(string folderPattern) {
            var thisNamespace = GetType().Namespace;
            var folderNamespace = $"{thisNamespace}.{folderPattern}";
            
            var sqlFiles = GetType().Assembly
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(folderNamespace) && SqlExtensions.Contains(new FileInfo(name).Extension))
                .Select(name => GetType().Assembly.GetManifestResourceStream(name))
                .Select(stream => {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        return streamReader.ReadToEnd();
                    }
                })
                .ToList();

            return sqlFiles;
        }
    }
}