using System;

namespace Dalion.HttpMessageSigning.Utils {
    /// <summary>
    ///     Represents the clock of the current system.
    /// </summary>
    public interface ISystemClock {
        /// <summary>
        ///     Gets the current system <see cref="T:System.DateTimeOffset" /> object whose date and time are set to the current Coordinated Universal Time (UTC) date and time and whose offset is <see cref="F:System.TimeSpan.Zero" />.
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}