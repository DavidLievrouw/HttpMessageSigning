using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class HeaderAppenderFactory : IHeaderAppenderFactory {
        public IHeaderAppender Create(HttpRequestForSigning request, SigningSettings settings, DateTimeOffset timeOfComposing) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            return new CompositeHeaderAppender(
                new DefaultHeaderAppender(request), 
                new RequestTargetHeaderAppender(request), 
                new CreatedHeaderAppender(settings, timeOfComposing),
                new ExpiresHeaderAppender(settings, timeOfComposing),
                new DateHeaderAppender(request));
        }
    }
}