using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class RealSystemClock : ISystemClock {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}