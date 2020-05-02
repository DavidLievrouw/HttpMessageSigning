using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private readonly DateTimeOffset? _timeOfComposing;

        public CreatedHeaderAppender(DateTimeOffset? timeOfComposing) {
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (!_timeOfComposing.HasValue) return string.Empty;
            var createdValue = _timeOfComposing.Value.ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
        }
    }
}