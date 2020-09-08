using System;
using Dalion.HttpMessageSigning.SigningString.RequestTarget;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class HeaderAppenderFactory : IHeaderAppenderFactory {
        private readonly IRequestTargetEscaper _requestTargetEscaper;
        
        public HeaderAppenderFactory(IRequestTargetEscaper requestTargetEscaper) {
            _requestTargetEscaper = requestTargetEscaper ?? throw new ArgumentNullException(nameof(requestTargetEscaper));
        }

        public IHeaderAppender Create(HttpRequestForSignatureString request, RequestTargetEscaping requestTargetEscaping, DateTimeOffset? timeOfComposing, TimeSpan? expires) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new CompositeHeaderAppender(
                new DefaultHeaderAppender(request), 
                new RequestTargetHeaderAppender(request, requestTargetEscaping, _requestTargetEscaper), 
                new CreatedHeaderAppender(timeOfComposing),
                new ExpiresHeaderAppender(timeOfComposing, expires),
                new DateHeaderAppender(request));
        }
    }
}