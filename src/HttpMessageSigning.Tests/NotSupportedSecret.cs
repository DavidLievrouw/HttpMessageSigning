using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    public class NotSupportedSignatureAlgorithm : ISignatureAlgorithm {
        public void Dispose() { }
        
        public string Name => "NOTSUPPORTED";
        
        public HashAlgorithmName HashAlgorithm { get; set; }
        
        public byte[] ComputeHash(string contentToSign) {
            throw new NotSupportedException();
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            throw new NotSupportedException();
        }
    }
}