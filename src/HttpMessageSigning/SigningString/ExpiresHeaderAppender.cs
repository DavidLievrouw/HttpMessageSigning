using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private readonly TimeSpan? _expires;
        private readonly DateTimeOffset? _timeOfComposing;

        public ExpiresHeaderAppender(DateTimeOffset? timeOfComposing, TimeSpan? expires) {
            _timeOfComposing = timeOfComposing;
            _expires = expires;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (!_expires.HasValue) return string.Empty;
            if (!_timeOfComposing.HasValue) return string.Empty;
            var expiresValue = _timeOfComposing.Value.Add(_expires.Value).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}