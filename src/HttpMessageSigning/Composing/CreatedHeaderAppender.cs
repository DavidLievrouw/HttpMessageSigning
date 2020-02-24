using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private readonly ISystemClock _systemClock;

        public CreatedHeaderAppender(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public string BuildStringToAppend(HeaderName header) {
            var createdValue = _systemClock.UtcNow.ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
        }
    }
}