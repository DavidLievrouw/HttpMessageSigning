using System;

namespace Dalion.HttpMessageSigning {
    internal class RealSystemClock : ISystemClock {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}