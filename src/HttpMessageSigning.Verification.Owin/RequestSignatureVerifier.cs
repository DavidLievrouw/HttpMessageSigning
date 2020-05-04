using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    internal class RequestSignatureVerifier : IRequestSignatureVerifier {
        private readonly ISignatureParser _signatureParser;
        private readonly IClientStore _clientStore;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IVerificationResultCreatorFactory _verificationResultCreatorFactory;
        private readonly ILogger<RequestSignatureVerifier> _logger;

        public RequestSignatureVerifier(
            ISignatureParser signatureParser,
            IClientStore clientStore,
            ISignatureVerifier signatureVerifier, 
            IVerificationResultCreatorFactory verificationResultCreatorFactory, 
            ILogger<RequestSignatureVerifier> logger = null) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _verificationResultCreatorFactory = verificationResultCreatorFactory ?? throw new ArgumentNullException(nameof(verificationResultCreatorFactory));
            _logger = logger;
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(IOwinRequest request, SignedHttpRequestAuthenticationOptions options) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (options == null) throw new ArgumentNullException(nameof(options));

            Client client = null;
            Signature signature = null;
            
            try {
                signature = _signatureParser.Parse(request, options);
                
                var eventTask = options.OnSignatureParsed;
                if (eventTask != null) await eventTask.Invoke(request, signature).ConfigureAwait(false);
                
                client = await _clientStore.Get(signature.KeyId).ConfigureAwait(false);

                var requestForSigning = request.ToHttpRequestForSigning();
                var verificationFailure = await _signatureVerifier.VerifySignature(requestForSigning, signature, client).ConfigureAwait(false);

                var verificationResultCreator = _verificationResultCreatorFactory.Create(client, signature);
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
            catch (InvalidClientException ex) {
                var failure = SignatureVerificationFailure.InvalidClient(ex.Message, ex);
                _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Code, failure.Message);
                return new RequestSignatureVerificationResultFailure(client, signature, failure);
            }
            catch (InvalidSignatureException ex) {
                var failure = SignatureVerificationFailure.InvalidSignature(ex.Message, ex);
                _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Code, failure.Message);
                return new RequestSignatureVerificationResultFailure(client, signature, failure);
            }
        }

        public void Dispose() {
            _clientStore?.Dispose();
        }
    }
}