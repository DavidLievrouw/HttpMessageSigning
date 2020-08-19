using System;

namespace Dalion.HttpMessageSigning.Verification {
    internal class VerificationResultCreator : IVerificationResultCreator {
        private readonly Client _client;
        private readonly HttpRequestForVerification _requestForVerification;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;

        public VerificationResultCreator(Client client, HttpRequestForVerification requestForVerification, IClaimsPrincipalFactory claimsPrincipalFactory) {
            _client = client; // Can be null when specifying an unknown client
            _requestForVerification = requestForVerification; // Can be null, because a failure might have occurred before extracting the data
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        }

        public RequestSignatureVerificationResult CreateForSuccess() {
            if (_client == null) throw new InvalidOperationException($"Cannot create a success {nameof(RequestSignatureVerificationResult)} without specifying a valid {nameof(Client)}.");
            if (_requestForVerification == null) throw new InvalidOperationException($"Cannot create a success {nameof(RequestSignatureVerificationResult)} without specifying a valid {nameof(HttpRequestForVerification)}.");
            if (_requestForVerification.Signature == null) throw new InvalidOperationException($"Cannot create a success {nameof(RequestSignatureVerificationResult)} without specifying a valid {nameof(Signature)} in the {nameof(HttpRequestForVerification)}.");
            return new RequestSignatureVerificationResultSuccess(_client, _requestForVerification, _claimsPrincipalFactory.CreateForClient(_client));
        }

        public RequestSignatureVerificationResult CreateForFailure(SignatureVerificationFailure failure) {
            if (failure == null) throw new ArgumentNullException(nameof(failure));
            return new RequestSignatureVerificationResultFailure(_client, _requestForVerification, failure);
        }
    }
}