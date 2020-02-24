using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSigner : IRequestSigner {
        private const string AuthorizationScheme = "Signature";
        
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;

        public RequestSigner(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            IHttpMessageSigningLogger<RequestSigner> logger) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Sign(HttpRequestMessage request, SigningSettings signingSettings) {
            try {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
                
                signingSettings.Validate();

                var signature = _signatureCreator.CreateSignature(request, signingSettings);
                var authParam = _authorizationHeaderParamCreator.CreateParam(signature);

                _logger.Debug("Setting Authorization scheme to '{0}' and param to '{1}'.", AuthorizationScheme, authParam);

                request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, authParam);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Could not sign the specified request. See inner exception.");
                throw;
            }
        }
    }
}