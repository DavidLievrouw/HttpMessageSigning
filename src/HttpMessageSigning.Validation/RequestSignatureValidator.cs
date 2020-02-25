using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Validation {
    internal class RequestSignatureValidator : IRequestSignatureValidator {
        private readonly ISignatureParser _signatureParser;

        public RequestSignatureValidator(ISignatureParser signatureParser) {
            _signatureParser = signatureParser ?? throw new ArgumentNullException(nameof(signatureParser));
        }

        public Task<RequestSignatureValidationResult> ValidateSignature(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                var signature = _signatureParser.Parse(request);

                // ToDo: Implement validation here

                return Task.FromResult<RequestSignatureValidationResult>(
                    new RequestSignatureValidationResultSuccess(
                        new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new[] {
                                    new Claim(Constants.ClaimTypes.AppId, signature.KeyId)
                                },
                                Constants.AuthenticationSchemes.Signature,
                                Constants.ClaimTypes.AppId,
                                Constants.ClaimTypes.Role))
                    ));
            }
            catch (SignatureValidationException ex) {
                return Task.FromResult<RequestSignatureValidationResult>(
                    new RequestSignatureValidationResultFailure(ex));
            }
        }
    }
}