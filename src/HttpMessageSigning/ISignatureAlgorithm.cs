using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    public interface ISignatureAlgorithm : IDisposable {
        string Name { get; }
        HashAlgorithmName HashAlgorithm { get; }
        byte[] ComputeHash(string contentToSign);
        bool VerifySignature(string contentToSign, byte[] signature);
    }
}