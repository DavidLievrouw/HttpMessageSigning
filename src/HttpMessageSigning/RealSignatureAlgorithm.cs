using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class RealSignatureAlgorithm : ISignatureAlgorithm {
        private readonly KeyedHashAlgorithm _realAlgorithm;
        
        public RealSignatureAlgorithm(string name, KeyedHashAlgorithm realAlgorithm) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _realAlgorithm = realAlgorithm ?? throw new ArgumentNullException(nameof(realAlgorithm));
        }

        public string Name { get; }

        public byte[] ComputeHash(string input) {
            return _realAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public byte[] Key => _realAlgorithm.Key;

        public void Dispose() {
            _realAlgorithm?.Dispose();
        }
    }
}