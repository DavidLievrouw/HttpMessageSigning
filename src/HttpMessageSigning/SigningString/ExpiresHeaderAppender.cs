using System;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private readonly TimeSpan? _expires;
        private readonly DateTimeOffset? _timeOfComposing;

        public ExpiresHeaderAppender(DateTimeOffset? timeOfComposing, TimeSpan? expires) {
            _timeOfComposing = timeOfComposing;
            _expires = expires;
        }
        
        public void Append(HeaderName header, StringBuilder sb) {
            if (!_expires.HasValue) return;
            if (!_timeOfComposing.HasValue) return;
            
            var expiresValue = _timeOfComposing.Value.Add(_expires.Value).ToUnixTimeSeconds();
            
            var headerToAppend = new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
            headerToAppend.Append(sb);
        }
    }
}