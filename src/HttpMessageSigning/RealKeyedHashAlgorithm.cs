using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class RealKeyedHashAlgorithm : IKeyedHashAlgorithm {
        private readonly KeyedHashAlgorithm _realAlgorithm;
        
        public RealKeyedHashAlgorithm(KeyedHashAlgorithm realAlgorithm) {
            _realAlgorithm = realAlgorithm ?? throw new ArgumentNullException(nameof(realAlgorithm));
        }

        public byte[] ComputeHash(string input) {
            return _realAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public byte[] Key => _realAlgorithm.Key;
    }
}