using System;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SigningStringCompositionRequestFactory : ISigningStringCompositionRequestFactory {
        public SigningStringCompositionRequest CreateForVerification(HttpRequestForSigning request, Client client, Signature signature) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var expires = signature.Created.HasValue && signature.Expires.HasValue 
                ? signature.Expires.Value - signature.Created.Value
                : new TimeSpan?();

            return new SigningStringCompositionRequest {
                Request = request,
                RequestTargetEscaping = RequestTargetEscaping.RFC3986, // ToDo #13
                HeadersToInclude = signature.Headers,
                TimeOfComposing = signature.Created,
                Expires = expires,
                Nonce = signature.Nonce
            };
        }
    }
}