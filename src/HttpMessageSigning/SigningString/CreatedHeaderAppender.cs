using System;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private readonly DateTimeOffset? _timeOfComposing;

        public CreatedHeaderAppender(DateTimeOffset? timeOfComposing) {
            _timeOfComposing = timeOfComposing;
        }

        public void Append(HeaderName header, StringBuilder sb) {
            if (!_timeOfComposing.HasValue) return;
            
            var createdValue = _timeOfComposing.Value.ToUnixTimeSeconds();
            
            var headerToAppend = new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
            headerToAppend.Append(sb);
        }
    }
}