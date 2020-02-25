using System;

namespace Dalion.HttpMessageSigning {
    internal interface IHashAlgorithm : IDisposable {
        HashAlgorithm Id { get; }
        string Name { get; }
        byte[] ComputeHash(byte[] input);
    }
}