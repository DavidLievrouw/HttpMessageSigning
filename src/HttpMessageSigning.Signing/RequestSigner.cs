using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSigner : IRequestSigner {
        private readonly ISigningSettingsSanitizer _signingSettingsSanitizer;
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly SigningSettings _signingSettings;
        private readonly ISignatureHeaderEnsurer _signatureHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<RequestSigner> _logger;

        public RequestSigner(
            ISigningSettingsSanitizer signingSettingsSanitizer,
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            SigningSettings signingSettings,
            ISignatureHeaderEnsurer signatureHeaderEnsurer, 
            ISystemClock systemClock,
            ILogger<RequestSigner> logger = null) {
            _signingSettingsSanitizer = signingSettingsSanitizer ?? throw new ArgumentNullException(nameof(signingSettingsSanitizer));
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _signingSettings = signingSettings ?? throw new ArgumentNullException(nameof(signingSettings));
            _signatureHeaderEnsurer = signatureHeaderEnsurer ?? throw new ArgumentNullException(nameof(signatureHeaderEnsurer));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            _logger = logger;
        }

        public Task Sign(HttpRequestMessage request) {
            return Sign(request, _systemClock.UtcNow, _signingSettings.Expires);
        }

        public async Task Sign(HttpRequestMessage request, DateTimeOffset timeOfSigning, TimeSpan expires) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            try {
                if (timeOfSigning > _systemClock.UtcNow) {
                    throw new HttpMessageSigningException("Cannot create a signature that is created in the future.");
                }
                
                if (timeOfSigning.Add(expires) < _systemClock.UtcNow) {
                    throw new HttpMessageSigningException("Cannot create a signature that has already expired.");
                }
                
                var clonedSettings = (SigningSettings)_signingSettings.Clone();
                var onRequestSigningTask = _signingSettings.Events?.OnRequestSigning?.Invoke(request, clonedSettings);
                if (onRequestSigningTask != null) await onRequestSigningTask.ConfigureAwait(continueOnCapturedContext: false);
                _signingSettingsSanitizer.SanitizeHeaderNamesToInclude(clonedSettings, request);
                
                clonedSettings.Validate();
                
                await _signatureHeaderEnsurer.EnsureHeader(request, clonedSettings, timeOfSigning).ConfigureAwait(continueOnCapturedContext: false);
                
                var signature = await _signatureCreator.CreateSignature(request, clonedSettings, timeOfSigning, expires).ConfigureAwait(continueOnCapturedContext: false);
                var onSignatureCreatedTask = _signingSettings.Events?.OnSignatureCreated?.Invoke(request, signature, clonedSettings);
                if (onSignatureCreatedTask != null) await onSignatureCreatedTask.ConfigureAwait(continueOnCapturedContext: false);
                
                var authParam = _authorizationHeaderParamCreator.CreateParam(signature);

                _logger?.LogDebug("Setting Authorization scheme to '{0}' and param to '{1}'.", clonedSettings.AuthorizationScheme, authParam);

                request.Headers.Authorization = new AuthenticationHeaderValue(clonedSettings.AuthorizationScheme, authParam);
                
                var onRequestSignedTask = _signingSettings.Events?.OnRequestSigned?.Invoke(request, signature, clonedSettings);
                if (onRequestSignedTask != null) await onRequestSignedTask.ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex) {
                _logger?.LogError(ex, "Could not sign the specified request. See inner exception.");
                throw;
            }
        }

        public void Dispose() {
            _signingSettings?.Dispose();
        }
    }
}