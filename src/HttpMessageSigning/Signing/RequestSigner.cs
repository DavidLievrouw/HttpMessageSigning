using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSigner : IRequestSigner {
        private const string AuthorizationScheme = "Signature";
        
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;

        public RequestSigner(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
        }

        public void Sign(HttpRequestMessage request, SigningSettings signingSettings) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            signingSettings.Validate();
            
            var signature = _signatureCreator.CreateSignature(request, signingSettings);
            var authParam = _authorizationHeaderParamCreator.CreateParam(signature);
            
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, authParam);
        }
    }
}