using System;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning;

namespace Conformance {
    public class CustomSignatureAlgorithm : ISignatureAlgorithm {
        private bool _verificationResult;
        private bool _isDisposed;

        public CustomSignatureAlgorithm(string name) {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            _verificationResult = true;
            _isDisposed = false;
        }

        public void Dispose() {
            _isDisposed = true;
        }

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

        public bool IsDisposed() {
            return _isDisposed;
        }
    }
}