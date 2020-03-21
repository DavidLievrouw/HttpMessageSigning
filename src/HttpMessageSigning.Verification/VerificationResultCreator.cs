using System;

namespace Dalion.HttpMessageSigning.Verification {
    internal class VerificationResultCreator : IVerificationResultCreator {
        private readonly Client _client;
        private readonly Signature _signature;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;

        public VerificationResultCreator(Client client, Signature signature, IClaimsPrincipalFactory claimsPrincipalFactory) {
            _client = client; // Can be null when specifying an unknown client
            _signature = signature; // Can be null when the signature cannot be parsed
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        }

        public RequestSignatureVerificationResult CreateForSuccess() {
            if (_client == null) throw new InvalidOperationException($"Cannot create a success {nameof(RequestSignatureVerificationResult)} without specifying a valid {nameof(Client)}.");
            if (_signature == null) throw new InvalidOperationException($"Cannot create a success {nameof(RequestSignatureVerificationResult)} without specifying a valid {nameof(Signature)}.");
            return new RequestSignatureVerificationResultSuccess(_client, _signature, _claimsPrincipalFactory.CreateForClient(_client));
        }

        public RequestSignatureVerificationResult CreateForFailure(SignatureVerificationFailure failure) {
            if (failure == null) throw new ArgumentNullException(nameof(failure));
            return new RequestSignatureVerificationResultFailure(_client, _signature, failure);
        }
    }
}