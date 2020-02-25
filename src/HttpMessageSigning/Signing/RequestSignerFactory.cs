using System;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSignerFactory : IRequestSignerFactory {
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly IAdditionalSignatureHeadersSetter _additionalSignatureHeadersSetter;
        private readonly ISystemClock _systemClock;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;
        
        public RequestSignerFactory(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            IAdditionalSignatureHeadersSetter additionalSignatureHeadersSetter,
            ISystemClock systemClock,
            IHttpMessageSigningLogger<RequestSigner> logger) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _additionalSignatureHeadersSetter = additionalSignatureHeadersSetter ?? throw new ArgumentNullException(nameof(additionalSignatureHeadersSetter));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }
        
        public IRequestSigner Create(SigningSettings signingSettings) {
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            signingSettings.Validate();
            return new RequestSigner(
                _signatureCreator, 
                _authorizationHeaderParamCreator, 
                signingSettings,
                _additionalSignatureHeadersSetter,
                _systemClock,
                _logger);
        }
    }
}