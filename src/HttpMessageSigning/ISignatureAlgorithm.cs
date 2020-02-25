using System;

namespace Dalion.HttpMessageSigning {
    internal interface ISignatureAlgorithm : IDisposable {
        string Name { get; }
        byte[] ComputeHash(string input);
        byte[] Key { get; }
    }
}