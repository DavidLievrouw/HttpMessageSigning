using System;

namespace Dalion.HttpMessageSigning {
    internal interface ISignatureAlgorithm : IDisposable {
        byte[] ComputeHash(string contentToSign);
        bool VerifySignature(string contentToSign, byte[] signature);
    }
}