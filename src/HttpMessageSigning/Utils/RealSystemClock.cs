using System;

namespace Dalion.HttpMessageSigning.Utils {
    internal class RealSystemClock : ISystemClock {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}