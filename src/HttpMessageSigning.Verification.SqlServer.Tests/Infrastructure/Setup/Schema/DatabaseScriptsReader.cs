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

        private readonly DirectoryInfo _scriptsDirectory;

        public DatabaseScriptsReader(string relativeScriptsFolder) {
            var workingDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            _scriptsDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(workingDirectory, relativeScriptsFolder)));
        }

        public IEnumerable<string> ReadAllDatabaseScriptsInOrder() {
            return from folderPattern in FolderPatterns
                from sql in ReadAllSqlFiles(folderPattern)
                select sql;
        }

        private IEnumerable<string> ReadAllSqlFiles(string folderPattern) {
            var matchedDirectories = _scriptsDirectory.GetDirectories(folderPattern, SearchOption.AllDirectories);
            var sqlFiles = matchedDirectories
                .SelectMany(d => d.EnumerateFiles())
                .Where(f => SqlExtensions.Contains(f.Extension));
            return sqlFiles.Select(f => File.ReadAllText(f.FullName));
        }
    }
}