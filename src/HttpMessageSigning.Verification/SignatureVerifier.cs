using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureVerifier : ISignatureVerifier {
        private readonly ISignatureSanitizer _signatureSanitizer;
        private readonly IVerificationTask _knownAlgorithmVerificationTask;
        private readonly IVerificationTask _matchingAlgorithmVerificationTask;
        private readonly IVerificationTask _createdHeaderGuardVerificationTask;
        private readonly IVerificationTask _expiresHeaderGuardVerificationTask;
        private readonly IVerificationTask _allHeadersPresentVerificationTask;
        private readonly IVerificationTask _creationTimeVerificationTask;
        private readonly IVerificationTask _expirationTimeVerificationTask;
        private readonly IVerificationTask _nonceVerificationTask;
        private readonly IVerificationTask _digestVerificationTask;
        private readonly IVerificationTask _matchingSignatureStringVerificationTask;

        public SignatureVerifier(
            ISignatureSanitizer signatureSanitizer,
            IVerificationTask knownAlgorithmVerificationTask,
            IVerificationTask matchingAlgorithmVerificationTask,
            IVerificationTask createdHeaderGuardVerificationTask,
            IVerificationTask expiresHeaderGuardVerificationTask,
            IVerificationTask allHeadersPresentVerificationTask,
            IVerificationTask creationTimeVerificationTask,
            IVerificationTask expirationTimeVerificationTask,
            IVerificationTask nonceVerificationTask,
            IVerificationTask digestVerificationTask,
            IVerificationTask matchingSignatureStringVerificationTask) {
            _signatureSanitizer = signatureSanitizer ?? throw new ArgumentNullException(nameof(signatureSanitizer));
            _knownAlgorithmVerificationTask = knownAlgorithmVerificationTask ?? throw new ArgumentNullException(nameof(knownAlgorithmVerificationTask));
            _matchingAlgorithmVerificationTask = matchingAlgorithmVerificationTask ?? throw new ArgumentNullException(nameof(matchingAlgorithmVerificationTask));
            _createdHeaderGuardVerificationTask = createdHeaderGuardVerificationTask ?? throw new ArgumentNullException(nameof(createdHeaderGuardVerificationTask));
            _expiresHeaderGuardVerificationTask = expiresHeaderGuardVerificationTask ?? throw new ArgumentNullException(nameof(expiresHeaderGuardVerificationTask));
            _allHeadersPresentVerificationTask = allHeadersPresentVerificationTask ?? throw new ArgumentNullException(nameof(allHeadersPresentVerificationTask));
            _creationTimeVerificationTask = creationTimeVerificationTask ?? throw new ArgumentNullException(nameof(creationTimeVerificationTask));
            _expirationTimeVerificationTask = expirationTimeVerificationTask ?? throw new ArgumentNullException(nameof(expirationTimeVerificationTask));
            _digestVerificationTask = digestVerificationTask ?? throw new ArgumentNullException(nameof(digestVerificationTask));
            _nonceVerificationTask = nonceVerificationTask ?? throw new ArgumentNullException(nameof(nonceVerificationTask));
            _matchingSignatureStringVerificationTask = matchingSignatureStringVerificationTask ?? throw new ArgumentNullException(nameof(matchingSignatureStringVerificationTask));
        }

        public async Task<SignatureVerificationFailure> VerifySignature(HttpRequestForVerification signedRequest, Client client) {
            if (signedRequest == null) throw new ArgumentNullException(nameof(signedRequest));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var sanitizedSignature = await _signatureSanitizer.Sanitize(signedRequest.Signature, client).ConfigureAwait(false);
            
            var failure = await _knownAlgorithmVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _matchingAlgorithmVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false)??
                          await _createdHeaderGuardVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _expiresHeaderGuardVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _allHeadersPresentVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _creationTimeVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _expirationTimeVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _nonceVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _digestVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false) ??
                          await _matchingSignatureStringVerificationTask.Verify(signedRequest, sanitizedSignature, client).ConfigureAwait(false);

            return failure;
        }
    }
}