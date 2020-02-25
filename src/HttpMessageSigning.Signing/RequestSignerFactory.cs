using System;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSignerFactory : IRequestSignerFactory {
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;
        
        public RequestSignerFactory(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            ISignatureHeaderEnsurer dateHeaderEnsurer, 
            ISignatureHeaderEnsurer digestHeaderEnsurer,
            ISystemClock systemClock,
            IHttpMessageSigningLogger<RequestSigner> logger) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _dateHeaderEnsurer = dateHeaderEnsurer ?? throw new ArgumentNullException(nameof(dateHeaderEnsurer));
            _digestHeaderEnsurer = digestHeaderEnsurer ?? throw new ArgumentNullException(nameof(digestHeaderEnsurer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }
        
        public IRequestSigner Create(SigningSettings signingSettings) {
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            
             signingSettings.Validate();
            
            return new RequestSigner(
                _signatureCreator, 
                _authorizationHeaderParamCreator, 
                signingSettings,
                _dateHeaderEnsurer,
                _digestHeaderEnsurer,
                _systemClock,
                _logger);
        }
    }
}