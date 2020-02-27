using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Logging;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification {
    internal class RequestSignatureVerifier : IRequestSignatureVerifier {
        private readonly ISignatureParser _signatureParser;
        private readonly IClientStore _clientStore;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly ISignatureSanitizer _signatureSanitizer;
        private readonly IHttpMessageSigningLogger<RequestSignatureVerifier> _logger;

        public RequestSignatureVerifier(
            ISignatureParser signatureParser,
            IClientStore clientStore,
            ISignatureVerifier signatureVerifier,
            IClaimsPrincipalFactory claimsPrincipalFactory,
            ISignatureSanitizer signatureSanitizer,
            IHttpMessageSigningLogger<RequestSignatureVerifier> logger) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
            _signatureSanitizer = signatureSanitizer ?? throw new ArgumentNullException(nameof(signatureSanitizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            RequestSignatureVerificationResult result;
            Client client = null;
            
            try {
                var signature = _signatureParser.Parse(request);
                client = await _clientStore.Get(signature.KeyId);

                var requestForSigning = await request.ToRequestForSigning(client.SignatureAlgorithm, signature);

                var sanitizedSignature = await _signatureSanitizer.Sanitize(signature, client);
                var verificationFailure = await _signatureVerifier.VerifySignature(requestForSigning, sanitizedSignature, client);

                if (verificationFailure != null && !(verificationFailure is SignatureVerificationException)) {
                    throw verificationFailure;
                }

                result = verificationFailure == null
                    ? (RequestSignatureVerificationResult) new RequestSignatureVerificationResultSuccess(client, _claimsPrincipalFactory.CreateForClient(client))
                    : (RequestSignatureVerificationResult) new RequestSignatureVerificationResultFailure(client, (SignatureVerificationException) verificationFailure);
            }
            catch (SignatureVerificationException ex) {
                result = new RequestSignatureVerificationResultFailure(client, ex);
            }

            if (result is RequestSignatureVerificationResultSuccess success) {
                _logger.Debug($"Request verification succeeded for principal {success.Principal?.Identity?.Name ?? "[null]"}.");
            }
            else if (result is RequestSignatureVerificationResultFailure failure) {
                _logger.Warning(failure.SignatureVerificationException, "Request verification failed. See exception for details.");
            }
            
            return result;
        }

        public virtual void Dispose() {
            _clientStore?.Dispose();
        }
    }
}