using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSigner : IRequestSigner {
        private const string AuthorizationScheme = "Signature";
        
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly SigningSettings _signingSettings;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;

        public RequestSigner(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            SigningSettings signingSettings,
            IHttpMessageSigningLogger<RequestSigner> logger) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _signingSettings = signingSettings ?? throw new ArgumentNullException(nameof(signingSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Sign(HttpRequestMessage request) {
            try {
                if (request == null) throw new ArgumentNullException(nameof(request));
                
                _signingSettings.Validate();

                /*
                 if (!settings.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                settings.Headers = new[] {HeaderName.PredefinedHeaderNames.RequestTarget}.Concat(settings.Headers).ToArray();
            }

            if (!settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                // ToDo: Add Date header to request, if not specified
                var requestTargetHeaderIdx = Array.IndexOf(settings.Headers, HeaderName.PredefinedHeaderNames.RequestTarget);
                settings.Headers = settings.Headers
                    .Take(requestTargetHeaderIdx + 1)
                    .Concat(new[] {HeaderName.PredefinedHeaderNames.Date})
                    .Concat(settings.Headers.Skip(requestTargetHeaderIdx + 1))
                    .ToArray();
            }

            if (settings.DigestHashAlgorithm != HashAlgorithm.None && request.Method != HttpMethod.Get && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                // ToDo: Add Digest header to request, if not specified
                settings.Headers = settings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Digest}).ToArray();
            }
            
                 */
                var signature = _signatureCreator.CreateSignature(request, _signingSettings);
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