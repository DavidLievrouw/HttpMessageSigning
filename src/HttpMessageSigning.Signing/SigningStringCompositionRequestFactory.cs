using System;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Signing {
    internal class SigningStringCompositionRequestFactory : ISigningStringCompositionRequestFactory {
        private readonly INonceGenerator _nonceGenerator;

        public SigningStringCompositionRequestFactory(INonceGenerator nonceGenerator) {
            _nonceGenerator = nonceGenerator ?? throw new ArgumentNullException(nameof(nonceGenerator));
        }

        public SigningStringCompositionRequest CreateForSigning(
            HttpRequestForSigning request, 
            SigningSettings signingSettings, 
            DateTimeOffset? timeOfComposing,
            TimeSpan? expires) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            return new SigningStringCompositionRequest {
                Request = request,
                RequestTargetEscaping = signingSettings.RequestTargetEscaping,
                HeadersToInclude = signingSettings.Headers,
                TimeOfComposing = timeOfComposing,
                Expires = expires,
                Nonce = signingSettings.EnableNonce
                    ? _nonceGenerator.GenerateNonce()
                    : null
            };
        }
    }
}