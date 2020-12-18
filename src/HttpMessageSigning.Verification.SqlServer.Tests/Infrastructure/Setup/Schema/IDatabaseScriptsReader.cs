using System.Collections.Generic;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.Schema {
    internal interface IDatabaseScriptsReader {
        IEnumerable<string> ReadAllDatabaseScriptsInOrder();
    }
}