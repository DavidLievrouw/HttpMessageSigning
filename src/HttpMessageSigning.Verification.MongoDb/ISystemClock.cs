using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal interface ISystemClock {
        DateTimeOffset UtcNow { get; }
    }
}