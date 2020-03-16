using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    internal class RequestSignatureVerifier : IRequestSignatureVerifier {
        private readonly ISignatureParser _signatureParser;
        private readonly IClientStore _clientStore;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly ISignatureSanitizer _signatureSanitizer;
        private readonly ILogger<RequestSignatureVerifier> _logger;

        public RequestSignatureVerifier(
            ISignatureParser signatureParser,
            IClientStore clientStore,
            ISignatureVerifier signatureVerifier,
            IClaimsPrincipalFactory claimsPrincipalFactory,
            ISignatureSanitizer signatureSanitizer,
            ILogger<RequestSignatureVerifier> logger = null) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
            _signatureSanitizer = signatureSanitizer ?? throw new ArgumentNullException(nameof(signatureSanitizer));
            _logger = logger;
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(IOwinRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            RequestSignatureVerificationResult result;
            Client client = null;
            Signature signature = null;
            
            try {
                signature = _signatureParser.Parse(request);
                client = await _clientStore.Get(signature.KeyId);

                var requestForSigning = request.ToHttpRequestForSigning();

                var sanitizedSignature = await _signatureSanitizer.Sanitize(signature, client);
                var verificationFailure = await _signatureVerifier.VerifySignature(requestForSigning, sanitizedSignature, client);

                if (verificationFailure != null && !(verificationFailure is SignatureVerificationException)) {
                    throw verificationFailure;
                }

                result = verificationFailure == null
                    ? (RequestSignatureVerificationResult) new RequestSignatureVerificationResultSuccess(client, signature, _claimsPrincipalFactory.CreateForClient(client))
                    : (RequestSignatureVerificationResult) new RequestSignatureVerificationResultFailure(client, signature, (SignatureVerificationException) verificationFailure);
            }
            catch (SignatureVerificationException ex) {
                result = new RequestSignatureVerificationResultFailure(client, signature, ex);
            }

            if (result is RequestSignatureVerificationResultSuccess success) {
                _logger?.LogDebug($"Request verification succeeded for principal {success.Principal?.Identity?.Name ?? "[null]"}.");
            }
            else if (result is RequestSignatureVerificationResultFailure failure) {
                _logger?.LogWarning(failure.SignatureVerificationException, "Request verification failed. See exception for details.");
            }
            
            return result;
        }

        public void Dispose() {
            _clientStore?.Dispose();
        }
    }
}