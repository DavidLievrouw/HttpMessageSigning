using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureVerifier : ISignatureVerifier {
        private readonly IVerificationTask _knownAlgorithmVerificationTask;
        private readonly IVerificationTask _matchingAlgorithmVerificationTask;
        private readonly IVerificationTask _createdHeaderGuardVerificationTask;
        private readonly IVerificationTask _expiresHeaderGuardVerificationTask;
        private readonly IVerificationTask _allHeadersPresentVerificationTask;
        private readonly IVerificationTask _creationTimeVerificationTask;
        private readonly IVerificationTask _expirationTimeVerificationTask;
        private readonly IVerificationTask _matchingSignatureVerificationTask;

        public SignatureVerifier(
            IVerificationTask knownAlgorithmVerificationTask,
            IVerificationTask matchingAlgorithmVerificationTask,
            IVerificationTask createdHeaderGuardVerificationTask,
            IVerificationTask expiresHeaderGuardVerificationTask,
            IVerificationTask allHeadersPresentVerificationTask,
            IVerificationTask creationTimeVerificationTask,
            IVerificationTask expirationTimeVerificationTask,
            IVerificationTask matchingSignatureVerificationTask) {
            _knownAlgorithmVerificationTask = knownAlgorithmVerificationTask ?? throw new ArgumentNullException(nameof(knownAlgorithmVerificationTask));
            _matchingAlgorithmVerificationTask = matchingAlgorithmVerificationTask ?? throw new ArgumentNullException(nameof(matchingAlgorithmVerificationTask));
            _createdHeaderGuardVerificationTask = createdHeaderGuardVerificationTask ?? throw new ArgumentNullException(nameof(createdHeaderGuardVerificationTask));
            _expiresHeaderGuardVerificationTask = expiresHeaderGuardVerificationTask ?? throw new ArgumentNullException(nameof(expiresHeaderGuardVerificationTask));
            _allHeadersPresentVerificationTask = allHeadersPresentVerificationTask ?? throw new ArgumentNullException(nameof(allHeadersPresentVerificationTask));
            _creationTimeVerificationTask = creationTimeVerificationTask ?? throw new ArgumentNullException(nameof(creationTimeVerificationTask));
            _expirationTimeVerificationTask = expirationTimeVerificationTask ?? throw new ArgumentNullException(nameof(expirationTimeVerificationTask));
            _matchingSignatureVerificationTask = matchingSignatureVerificationTask ?? throw new ArgumentNullException(nameof(matchingSignatureVerificationTask));
        }

        public async Task VerifySignature(HttpRequestMessage signedRequest, Signature signature, Client client) {
            if (signedRequest == null) throw new ArgumentNullException(nameof(signedRequest));
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (client == null) throw new ArgumentNullException(nameof(client));

            await _knownAlgorithmVerificationTask.Verify(signedRequest, signature, client);
            await _matchingAlgorithmVerificationTask.Verify(signedRequest, signature, client);
            await _createdHeaderGuardVerificationTask.Verify(signedRequest, signature, client);
            await _expiresHeaderGuardVerificationTask.Verify(signedRequest, signature, client);
            await _allHeadersPresentVerificationTask.Verify(signedRequest, signature, client);
            await _creationTimeVerificationTask.Verify(signedRequest, signature, client);
            await _expirationTimeVerificationTask.Verify(signedRequest, signature, client);
            await _matchingSignatureVerificationTask.Verify(signedRequest, signature, client);
        }
    }
}