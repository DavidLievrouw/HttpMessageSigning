﻿using System;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SigningStringCompositionRequestFactory : ISigningStringCompositionRequestFactory {
        public SigningStringCompositionRequest CreateForVerification(HttpRequestForVerification request, Client client, Signature signature) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var expires = signature.Created.HasValue && signature.Expires.HasValue 
                ? signature.Expires.Value - signature.Created.Value
                : new TimeSpan?();

            return new SigningStringCompositionRequest {
                Request = request.ToHttpRequestForSignatureString(),
                RequestTargetEscaping = client.RequestTargetEscaping,
                HeadersToInclude = signature.Headers,
                TimeOfComposing = signature.Created,
                Expires = expires,
                Nonce = signature.Nonce
            };
        }
    }
}