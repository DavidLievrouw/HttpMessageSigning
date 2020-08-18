using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification {
    internal class RequestSignatureVerificationOrchestrator : IRequestSignatureVerificationOrchestrator {
        private readonly IClientStore _clientStore;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IVerificationResultCreatorFactory _verificationResultCreatorFactory;
        private readonly ILogger<RequestSignatureVerificationOrchestrator> _logger;

        public RequestSignatureVerificationOrchestrator(
            IClientStore clientStore,
            ISignatureVerifier signatureVerifier,
            IVerificationResultCreatorFactory verificationResultCreatorFactory,
            ILogger<RequestSignatureVerificationOrchestrator> logger = null) {
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _verificationResultCreatorFactory = verificationResultCreatorFactory ?? throw new ArgumentNullException(nameof(verificationResultCreatorFactory));
            _logger = logger;
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(HttpRequestForSigning request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Client client = null;
            try {
                if (request.Signature == null) {
                    throw new InvalidSignatureException("The signature is missing.");
                }
                
                try {
                    request.Signature.Validate();
                }
                catch (ValidationException ex) {
                    throw new InvalidSignatureException("The signature is invalid. See inner exception.", ex);
                }
                
                client = await _clientStore.Get(request.Signature.KeyId).ConfigureAwait(continueOnCapturedContext: false);
                if (client == null) {
                    var failure = SignatureVerificationFailure.InvalidClient($"No {nameof(Client)}s with id '{request.Signature.KeyId}' are registered in the server store.");
                    _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Code, failure.Message);
                    return new RequestSignatureVerificationResultFailure(client, request.Signature, failure);
                }
                
                var verificationFailure = await _signatureVerifier.VerifySignature(request, client).ConfigureAwait(continueOnCapturedContext: false);

                var verificationResultCreator = _verificationResultCreatorFactory.Create(client, request.Signature);
                var result = verificationFailure == null
                    ? verificationResultCreator.CreateForSuccess()
                    : verificationResultCreator.CreateForFailure(verificationFailure);

                if (result is RequestSignatureVerificationResultSuccess success) {
                    _logger?.LogDebug($"Request signature verification succeeded for principal {success.Principal?.Identity?.Name ?? "[null]"}.");
                }
                else if (result is RequestSignatureVerificationResultFailure failure) {
                    _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Failure.Code, failure.Failure.Message);
                }

                return result;
            }
            catch (InvalidSignatureException ex) {
                var failure = SignatureVerificationFailure.InvalidSignature(ex.Message, ex);
                _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Code, failure.Message);
                return new RequestSignatureVerificationResultFailure(client, request.Signature, failure);
            }
        }

        public void Dispose() {
            _clientStore?.Dispose();
        }
    }
}