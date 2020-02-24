using System;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSignerFactory : IRequestSignerFactory {
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;
        
        public RequestSignerFactory(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            IHttpMessageSigningLogger<RequestSigner> logger) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public IRequestSigner Create(SigningSettings signingSettings) {
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            signingSettings.Validate();
            return new RequestSigner(_signatureCreator, _authorizationHeaderParamCreator, signingSettings, _logger);
        }
    }
}