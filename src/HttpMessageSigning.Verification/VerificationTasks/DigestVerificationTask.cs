using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class DigestVerificationTask : VerificationTask {
        private readonly IBase64Converter _base64Converter;
        private readonly ILogger<DigestVerificationTask> _logger;

        public DigestVerificationTask(
            IBase64Converter base64Converter,
            ILogger<DigestVerificationTask> logger = null) {
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _logger = logger;
        }

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest) &&
                !signedRequest.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                return SignatureVerificationFailure.HeaderMissing($"The {HeaderName.PredefinedHeaderNames.Digest} header is indicated as part of the signature, but it is not included in the request.");
            }

            if (!signedRequest.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                _logger?.LogDebug("{0} header verification is not required, because it is not present in the request to verify.", HeaderName.PredefinedHeaderNames.Digest);
                return null;
            }

            if (signedRequest.Body == null) {
                return SignatureVerificationFailure.InvalidDigestHeader($"The {HeaderName.PredefinedHeaderNames.Digest} header verification failed. The request has no body.");
            }

            var digestHeaderValue = signedRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Digest).FirstOrDefault();
            var digestParams = new List<string>();
            if (!string.IsNullOrEmpty(digestHeaderValue)) {
                var separatorIndex = digestHeaderValue.IndexOf('=');
                if (separatorIndex < 0 || separatorIndex >= digestHeaderValue.Length - 1) {
                    digestParams.Add(digestHeaderValue);
                }
                else {
                    digestParams.Add(digestHeaderValue.Substring(0, separatorIndex));
                    digestParams.Add(digestHeaderValue.Substring(separatorIndex + 1));
                }
            }
            
            if (digestParams.Count < 2) {
                return SignatureVerificationFailure.InvalidDigestHeader($"The {HeaderName.PredefinedHeaderNames.Digest} request header is invalid.");
            }

            if (!Constants.DigestHashAlgorithms.TryGetValue(digestParams[0], out var digestAlgorithmName)) {
                return SignatureVerificationFailure.InvalidDigestHeader($"The {HeaderName.PredefinedHeaderNames.Digest} algorithm name ({digestParams[0] ?? "[null]"}) is invalid.");
            }
            
            using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(digestAlgorithmName)) {
                if (hashAlgorithm == null) {
                    return SignatureVerificationFailure.InvalidDigestHeader($"The {HeaderName.PredefinedHeaderNames.Digest} algorithm name ({digestParams[0] ?? "[null]"}) is currently not supported.");
                }
                
                var payloadBytes = hashAlgorithm.ComputeHash(signedRequest.Body);
                var calculatedDigest = _base64Converter.ToBase64(payloadBytes);
                var receivedDigest = digestParams[1];

                if (calculatedDigest != receivedDigest) {
                    return SignatureVerificationFailure.InvalidDigestHeader("The digest header verification failed.");
                }
            }
            
            return null;
        }
    }
}