using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class HeaderAppenderFactory : IHeaderAppenderFactory {
        public IHeaderAppender Create(HttpRequestForSigning request, DateTimeOffset timeOfComposing) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            return new CompositeHeaderAppender(
                new DefaultHeaderAppender(request), 
                new RequestTargetHeaderAppender(request), 
                new CreatedHeaderAppender(request, timeOfComposing),
                new ExpiresHeaderAppender(request, timeOfComposing),
                new DateHeaderAppender(request));
        }
    }
}