using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    internal interface IHashAlgorithm : IDisposable {
        HashAlgorithmName Id { get; }
        string Name { get; }
        byte[] ComputeHash(byte[] input);
    }
}