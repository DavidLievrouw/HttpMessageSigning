using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    public class CustomSignatureAlgorithm : ISignatureAlgorithm {
        public CustomSignatureAlgorithm(string name) {
            if (string.IsNullOrEmpty(name)) name = "NOTSUPPORTED";
            Name = name;
        }
        
        public void Dispose() { }
        
        public string Name { get; }
        
        public HashAlgorithmName HashAlgorithm { get; set; }
        
        public byte[] ComputeHash(string contentToSign) {
            throw new NotSupportedException();
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            throw new NotSupportedException();
        }
    }
}