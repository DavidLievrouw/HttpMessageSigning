using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification {
    internal class RequestSignatureValidator : IRequestSignatureValidator {
        private readonly ISignatureParser _signatureParser;
        private readonly IClientStore _clientStore;
        private readonly ISignatureValidator _signatureValidator;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;

        public RequestSignatureValidator(
            ISignatureParser signatureParser,
            IClientStore clientStore,
            ISignatureValidator signatureValidator,
            IClaimsPrincipalFactory claimsPrincipalFactory) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _signatureValidator = signatureValidator ?? throw new ArgumentNullException(nameof(signatureValidator));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        }

        public async Task<RequestSignatureValidationResult> ValidateSignature(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                var signature = _signatureParser.Parse(request);

                var client = await _clientStore.Get(signature.KeyId);

                await _signatureValidator.ValidateSignature(signature, client);

                return new RequestSignatureValidationResultSuccess(_claimsPrincipalFactory.CreateForClient(client));
            }
            catch (SignatureValidationException ex) {
                return new RequestSignatureValidationResultFailure(ex);
            }
        }
    }
}