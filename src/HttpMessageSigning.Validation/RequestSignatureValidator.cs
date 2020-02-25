using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Validation {
    internal class RequestSignatureValidator : IRequestSignatureValidator {
        private readonly ISignatureParser _signatureParser;
        private readonly IClientStore _clientStore;

        public RequestSignatureValidator(
            ISignatureParser signatureParser,
            IClientStore clientStore) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
        }

        public async Task<RequestSignatureValidationResult> ValidateSignature(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                var signature = _signatureParser.Parse(request);

                var client = await _clientStore.Get(signature.KeyId);

                // ToDo: Implement validation here

                var additionalClaims = client.Claims?.Select(c => new System.Security.Claims.Claim(c.Type, c.Value)) ?? Enumerable.Empty<System.Security.Claims.Claim>();
                return new RequestSignatureValidationResultSuccess(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(
                            additionalClaims.Concat(
                                new[] {
                                    new System.Security.Claims.Claim(Constants.ClaimTypes.AppId, client.Id)
                                }),
                            Constants.AuthenticationSchemes.Signature,
                            Constants.ClaimTypes.AppId,
                            Constants.ClaimTypes.Role))
                );
            }
            catch (SignatureValidationException ex) {
                return new RequestSignatureValidationResultFailure(ex);
            }
        }
    }
}