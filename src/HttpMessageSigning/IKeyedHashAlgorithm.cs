using System;

namespace Dalion.HttpMessageSigning {
    internal interface IKeyedHashAlgorithm : IDisposable {
        string Name { get; }
        byte[] ComputeHash(string input);
        byte[] Key { get; }
    }
}