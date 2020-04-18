using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    [ExcludeFromCodeCoverage]
    public class CustomSignatureAlgorithm : ISignatureAlgorithm {
        private bool _verificationResult;
        
        public CustomSignatureAlgorithm(string name) {
            if (string.IsNullOrEmpty(name)) name = "NOTSUPPORTED";
            Name = name;
            _verificationResult = true;
        }

        public void Dispose() { }

        public string Name { get; }

        public HashAlgorithmName HashAlgorithm { get; set; }

        public void SetVerificationResult(bool result) {
            _verificationResult = result;
        }
        
        public byte[] ComputeHash(string contentToSign) {
            return new byte[] {1, 2, 3};
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            return _verificationResult;
        }
    }
}