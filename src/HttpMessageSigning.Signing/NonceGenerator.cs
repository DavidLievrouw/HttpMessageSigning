using System;

namespace Dalion.HttpMessageSigning.Signing {
    internal class NonceGenerator : INonceGenerator {
        public string GenerateNonce() {
            return Guid.NewGuid().ToString("N");
        }
    }
}