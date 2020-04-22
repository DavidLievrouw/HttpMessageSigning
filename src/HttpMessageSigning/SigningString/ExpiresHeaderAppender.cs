using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private readonly DateTimeOffset _timeOfComposing;
        private readonly TimeSpan _expires;

        public ExpiresHeaderAppender(DateTimeOffset timeOfComposing, TimeSpan expires) {
            _timeOfComposing = timeOfComposing;
            _expires = expires;
        }

        public string BuildStringToAppend(HeaderName header) {
            var expiresValue = _timeOfComposing.Add(_expires).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}