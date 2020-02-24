using System;

namespace Dalion.HttpMessageSigning {
    internal interface IHashAlgorithm : IDisposable {
        string Name { get; }
        byte[] ComputeHash(string input);
    }
}