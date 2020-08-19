using System;
using System.Security.Cryptography;
using Microsoft.Extensions.ObjectPool;

namespace Dalion.HttpMessageSigning {
    internal class PooledHashAlgorithmPolicy : IPooledObjectPolicy<HashAlgorithm> {
        private readonly Func<HashAlgorithm> _creator;
        
        public PooledHashAlgorithmPolicy(Func<HashAlgorithm> creator) {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        public HashAlgorithm Create() {
            return _creator.Invoke();
        }

        public bool Return(HashAlgorithm obj) {
            return true;
        }
    }
}