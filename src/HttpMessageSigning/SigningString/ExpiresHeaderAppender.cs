using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private readonly SigningSettings _settings;
        private readonly DateTimeOffset _timeOfComposing;

        public ExpiresHeaderAppender(SigningSettings settings, DateTimeOffset timeOfComposing) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            var expiresValue = _timeOfComposing.Add(_settings.Expires).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}