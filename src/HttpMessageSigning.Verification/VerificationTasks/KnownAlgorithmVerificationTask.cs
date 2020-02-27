using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class KnownAlgorithmVerificationTask : IVerificationTask {
        private readonly IHttpMessageSigningLogger<KnownAlgorithmVerificationTask> _logger;
        
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "hmac"};

        public KnownAlgorithmVerificationTask(IHttpMessageSigningLogger<KnownAlgorithmVerificationTask> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) {
                _logger.Debug("Algorithm verification is not required, because there is no algorithm specified in the signature.");
                return Task.FromResult<Exception>(null);
            }

            var parts = signature.Algorithm.Split('-');
            if (parts.Length < 2) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            if (!SupportedSignatureAlgorithmNames.Contains(parts[0])) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(parts[1].ToUpperInvariant());
            if (hashAlgorithm == null) {
                return new SignatureVerificationException($"The specified hash algorithm ({parts[1]}) is not supported.")
                    .ToTask<Exception>();
            }
            
            return Task.FromResult<Exception>(null);
        }
    }
}