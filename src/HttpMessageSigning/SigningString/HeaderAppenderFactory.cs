using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class HeaderAppenderFactory : IHeaderAppenderFactory {
        public IHeaderAppender Create(HttpRequestForSigning request, string signatureAlgorithmName, DateTimeOffset timeOfComposing, TimeSpan expires) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signatureAlgorithmName == null) throw new ArgumentNullException(nameof(signatureAlgorithmName));

            return new CompositeHeaderAppender(
                new DefaultHeaderAppender(request), 
                new RequestTargetHeaderAppender(request), 
                new CreatedHeaderAppender(signatureAlgorithmName, timeOfComposing),
                new ExpiresHeaderAppender(signatureAlgorithmName, timeOfComposing, expires),
                new DateHeaderAppender(request));
        }
    }
}