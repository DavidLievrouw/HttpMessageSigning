using System;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class RealHashAlgorithm : IHashAlgorithm {
        private readonly System.Security.Cryptography.HashAlgorithm _realAlgorithm;

        public RealHashAlgorithm(string name, System.Security.Cryptography.HashAlgorithm realAlgorithm) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _realAlgorithm = realAlgorithm ?? throw new ArgumentNullException(nameof(realAlgorithm));
        }

        public string Name { get; }

        public byte[] ComputeHash(string input) {
            return _realAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public void Dispose() {
            _realAlgorithm?.Dispose();
        }
    }
}