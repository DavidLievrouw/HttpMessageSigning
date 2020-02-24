using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class HeaderAppenderFactory : IHeaderAppenderFactory {
        private readonly IBase64Converter _base64Converter;
        private readonly IHashAlgorithmFactory _hashAlgorithmFactory;

        public HeaderAppenderFactory(IBase64Converter base64Converter, IHashAlgorithmFactory hashAlgorithmFactory) {
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _hashAlgorithmFactory = hashAlgorithmFactory ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
        }

        public IHeaderAppender Create(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfComposing) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            return new CompositeHeaderAppender(
                new DefaultHeaderAppender(request), 
                new RequestTargetHeaderAppender(request), 
                new CreatedHeaderAppender(timeOfComposing),
                new ExpiresHeaderAppender(settings, timeOfComposing),
                new DateHeaderAppender(request));
        }
    }
}