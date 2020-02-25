using System;

namespace Dalion.HttpMessageSigning {
    public interface ISignatureAlgorithm : IDisposable {
        string Name { get; }
        HashAlgorithm HashAlgorithm { get; }
        byte[] ComputeHash(string contentToSign);
        bool VerifySignature(string contentToSign, byte[] signature);
    }
}