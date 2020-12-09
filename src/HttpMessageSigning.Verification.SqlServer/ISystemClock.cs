using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface ISystemClock {
        DateTimeOffset UtcNow { get; }
    }
}