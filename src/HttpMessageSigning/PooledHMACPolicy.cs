using System;
using System.Security.Cryptography;
using Microsoft.Extensions.ObjectPool;

namespace Dalion.HttpMessageSigning {
    internal class PooledHMACPolicy : IPooledObjectPolicy<HMAC> {
        private readonly Func<HMAC> _creator;
        
        public PooledHMACPolicy(Func<HMAC> creator) {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        public HMAC Create() {
            return _creator.Invoke();
        }

        public bool Return(HMAC obj) {
            return true;
        }
    }
}