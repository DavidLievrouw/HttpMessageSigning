using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSigner : IRequestSigner {
        private const string AuthorizationScheme = "SignedHttpRequest";
        
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly SigningSettings _signingSettings;
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<RequestSigner> _logger;

        public RequestSigner(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            SigningSettings signingSettings,
            ISignatureHeaderEnsurer dateHeaderEnsurer, 
            ISignatureHeaderEnsurer digestHeaderEnsurer,
            ISystemClock systemClock,
            ILogger<RequestSigner> logger) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _signingSettings = signingSettings ?? throw new ArgumentNullException(nameof(signingSettings));
            _dateHeaderEnsurer = dateHeaderEnsurer ?? throw new ArgumentNullException(nameof(dateHeaderEnsurer));
            _digestHeaderEnsurer = digestHeaderEnsurer ?? throw new ArgumentNullException(nameof(digestHeaderEnsurer));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Sign(HttpRequestMessage request) {
            try {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var clonedSettings = (SigningSettings)_signingSettings.Clone();
                var onRequestSigningTask = _signingSettings.Events?.OnRequestSigning?.Invoke(request, clonedSettings);
                if (onRequestSigningTask != null) await onRequestSigningTask;
                
                clonedSettings.Validate();

                var timeOfSigning = _systemClock.UtcNow;
                await _dateHeaderEnsurer.EnsureHeader(request, clonedSettings, timeOfSigning);
                await _digestHeaderEnsurer.EnsureHeader(request, clonedSettings, timeOfSigning);
                
                var signature = _signatureCreator.CreateSignature(request, clonedSettings, timeOfSigning);
                var authParam = _authorizationHeaderParamCreator.CreateParam(signature);

                _logger.LogDebug("Setting Authorization scheme to '{0}' and param to '{1}'.", AuthorizationScheme, authParam);

                request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, authParam);
                
                var onRequestSignedTask = _signingSettings.Events?.OnRequestSigned?.Invoke(request, clonedSettings);
                if (onRequestSignedTask != null) await onRequestSignedTask;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Could not sign the specified request. See inner exception.");
                throw;
            }
        }

        public virtual void Dispose() {
            _signingSettings?.Dispose();
        }
    }
}