using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class RealSystemClock : ISystemClock {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}