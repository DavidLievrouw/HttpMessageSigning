using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification {
    internal class RequestSignatureVerifier : IRequestSignatureVerifier {
        private readonly ISignatureParser _signatureParser;
        private readonly IClientStore _clientStore;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;

        public RequestSignatureVerifier(
            ISignatureParser signatureParser,
            IClientStore clientStore,
            ISignatureVerifier signatureVerifier,
            IClaimsPrincipalFactory claimsPrincipalFactory) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        }

        public async Task<RequestSignatureVerificationResult> VerifySignature(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                var requestMessage = request.ToHttpRequestMessage();
                
                var signature = _signatureParser.Parse(requestMessage);

                var client = await _clientStore.Get(signature.KeyId);

                await _signatureVerifier.VerifySignature(requestMessage, signature, client);

                return new RequestSignatureVerificationResultSuccess(_claimsPrincipalFactory.CreateForClient(client));
            }
            catch (SignatureVerificationException ex) {
                return new RequestSignatureVerificationResultFailure(ex);
            }
        }
    }
}