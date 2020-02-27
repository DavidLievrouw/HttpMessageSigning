using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class DigestVerificationTask : IVerificationTask {
        private readonly IBase64Converter _base64Converter;
        
        public DigestVerificationTask(IBase64Converter base64Converter) {
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
        }

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest) &&
                !signedRequest.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                return new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Digest} header is indicated as part of the signature, but it is not included in the request.")
                    .ToTask<Exception>();
            }

            if (!signedRequest.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) return Task.FromResult<Exception>(null);
            
            if (signedRequest.Body == null) return new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Digest} header verification failed. The request has no body.")
                .ToTask<Exception>();

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
                return new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Digest} request header is invalid.")
                    .ToTask<Exception>();
            }

            if (!Constants.DigestHashAlgorithms.TryGetValue(digestParams[0], out var digestAlgorithmName)) {
                return new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Digest} algorithm name ({digestParams[0] ?? "[null]"}) is invalid.")
                    .ToTask<Exception>();
            }
            
            using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(digestAlgorithmName)) {
                if (hashAlgorithm == null) {
                    return new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Digest} algorithm name ({digestParams[0] ?? "[null]"}) is currently not supported.")
                        .ToTask<Exception>();
                }

                var bodyBytes = Encoding.UTF8.GetBytes(signedRequest.Body);
                
                var payloadBytes = hashAlgorithm.ComputeHash(bodyBytes);
                var calculatedDigest = _base64Converter.ToBase64(payloadBytes);
                var receivedDigest = digestParams[1];

                if (calculatedDigest != receivedDigest) {
                    return new SignatureVerificationException("The digest header verification failed.")
                        .ToTask<Exception>();
                }
            }
            
            return Task.FromResult<Exception>(null);
        }
    }
}