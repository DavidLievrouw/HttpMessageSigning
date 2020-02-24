using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private readonly ISystemClock _systemClock;
        private readonly SigningSettings _settings;

        public ExpiresHeaderAppender(ISystemClock systemClock, SigningSettings settings) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string BuildStringToAppend(HeaderName header) {
            var expiresValue = _systemClock.UtcNow.Add(_settings.Expires).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}