using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class NonceVerificationTask : IVerificationTask {
        private readonly INonceStore _nonceStore;
        private readonly ISystemClock _systemClock;

        public NonceVerificationTask(INonceStore nonceStore, ISystemClock systemClock) {
            _nonceStore = nonceStore ?? throw new ArgumentNullException(nameof(nonceStore));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public async Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (string.IsNullOrEmpty(signature.Nonce)) return null;

            var previousNonce = await _nonceStore.Get(client.Id, signature.Nonce);
            if (previousNonce != null && previousNonce.Expiration >= _systemClock.UtcNow) {
                return new SignatureVerificationException($"The nonce '{previousNonce.Value}' for client {client.Id} ({client.Name}) is not unique and has been used before. It expires at {previousNonce.Expiration:R}.");
            }
            
            var nonce = new Nonce(
                clientId: client.Id,
                value: signature.Nonce,
                expiration: _systemClock.UtcNow.Add(client.NonceLifetime));
            await _nonceStore.Register(nonce);

            return null;
        }
    }
}