using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal class HeaderAppenderFactory : IHeaderAppenderFactory {
        private readonly ISystemClock _systemClock;
        
        public HeaderAppenderFactory(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public IHeaderAppender Create(HttpRequestMessage request, SigningSettings settings) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            return new CompositeHeaderAppender(
                new DefaultHeaderAppender(request), 
                new RequestTargetHeaderAppender(request), 
                new CreatedHeaderAppender(_systemClock),
                new ExpiresHeaderAppender(_systemClock, settings),
                new DateHeaderAppender(request));
        }
    }
}