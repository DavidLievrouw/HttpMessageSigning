using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppender(DateTimeOffset timeOfComposing) {
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            var createdValue = _timeOfComposing.ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
        }
    }
}