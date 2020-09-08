using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class RequestSignatureVerifier : IRequestSignatureVerifier {
        private readonly IRequestSignatureVerificationOrchestrator _verificationOrchestrator;
        private readonly ILogger<RequestSignatureVerifier> _logger;
        private readonly ISignatureParser _signatureParser;

        public RequestSignatureVerifier(
            ISignatureParser signatureParser,
            IRequestSignatureVerificationOrchestrator verificationOrchestrator,
            ILogger<RequestSignatureVerifier> logger = null) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _verificationOrchestrator = verificationOrchestrator ?? throw new ArgumentNullException(nameof(verificationOrchestrator));
            _logger = logger;
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(HttpRequest request, SignedRequestAuthenticationOptions options) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var signatureParsingResult = _signatureParser.Parse(request, options);
            if (signatureParsingResult is SignatureParsingFailure parsingFailure) {
                var failure = SignatureVerificationFailure.InvalidSignature(parsingFailure.Description, parsingFailure.Failure);
                _logger?.LogWarning("Request signature verification failed ({0}): {1}", failure.Code, failure.Message);
                return new RequestSignatureVerificationResultFailure(client: null, requestForVerification: null, failure);
            }

            var parsedSignature = ((SignatureParsingSuccess)signatureParsingResult).Signature;
            
            var eventTask = options.OnSignatureParsed;
            if (eventTask != null) await eventTask.Invoke(request, parsedSignature).ConfigureAwait(continueOnCapturedContext: false);

            var requestForSigning = await request.ToHttpRequestForVerification(parsedSignature).ConfigureAwait(continueOnCapturedContext: false);
            return await _verificationOrchestrator.VerifySignature(requestForSigning).ConfigureAwait(continueOnCapturedContext: false);
        }

        public void Dispose() {
            _verificationOrchestrator?.Dispose();
        }
    }
}