using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class RequestSignatureVerifier : IRequestSignatureVerifier {
        private readonly IRequestSignatureVerificationOrchestrator _requestSignatureVerificationOrchestrator;
        private readonly ILogger<RequestSignatureVerifier> _logger;
        private readonly ISignatureParser _signatureParser;

        public RequestSignatureVerifier(
            ISignatureParser signatureParser,
            IRequestSignatureVerificationOrchestrator requestSignatureVerificationOrchestrator,
            ILogger<RequestSignatureVerifier> logger = null) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _requestSignatureVerificationOrchestrator = requestSignatureVerificationOrchestrator ?? throw new ArgumentNullException(nameof(requestSignatureVerificationOrchestrator));
            _logger = logger;
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(HttpRequest request, SignedRequestAuthenticationOptions options) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (options == null) throw new ArgumentNullException(nameof(options));

            Signature signature = null;
            try {
                signature = _signatureParser.Parse(request, options);
            }
            catch (InvalidSignatureException ex) {
                var failure = SignatureVerificationFailure.InvalidSignature(ex.Message, ex);
                _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Code, failure.Message);
                return new RequestSignatureVerificationResultFailure(client: null, signature, failure);
            }

            var eventTask = options.OnSignatureParsed;
            if (eventTask != null) await eventTask.Invoke(request, signature).ConfigureAwait(continueOnCapturedContext: false);

            var requestForSigning = await request.ToHttpRequestForSigning(signature).ConfigureAwait(continueOnCapturedContext: false);
            return await _requestSignatureVerificationOrchestrator.VerifySignature(requestForSigning);
        }

        public void Dispose() {
            _requestSignatureVerificationOrchestrator?.Dispose();
        }
    }
}