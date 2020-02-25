using System;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class RealHashAlgorithm : IHashAlgorithm {
        private readonly System.Security.Cryptography.HashAlgorithm _realAlgorithm;

        public RealHashAlgorithm(HashAlgorithm id, string name, System.Security.Cryptography.HashAlgorithm realAlgorithm) {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _realAlgorithm = realAlgorithm ?? throw new ArgumentNullException(nameof(realAlgorithm));
        }

        public HashAlgorithm Id { get; }
        
        public string Name { get; }

        public byte[] ComputeHash(byte[] input) {
            return _realAlgorithm.ComputeHash(input);
        }

        public void Dispose() {
            _realAlgorithm?.Dispose();
        }
    }
}