using System.Collections.Generic;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.OutdatedDatabases {
    internal interface IOutdatedDatabaseFinder {
        IEnumerable<string> FindOutdatedDatabases();
    }
}