using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a store that the server can use to check nonce validity, avoiding replay attacks.
    /// </summary>
    public interface INonceStore : IDisposable {
        /// <summary>
        /// Registers usage of the specified <see cref="Nonce"/> value.
        /// </summary>
        /// <param name="nonce">The <see cref="Nonce"/> that is received from a client.</param>
        Task Register(Nonce nonce);
        
        /// <summary>
        /// Gets the <see cref="Nonce"/> value with a matching string, that was previously sent by the <see cref="Client"/>, identified by the specified <see cref="KeyId"/>. 
        /// </summary>
        /// <param name="clientId">The identifier of the client to get the value for.</param>
        /// <param name="nonceValue">The nonce string value that was sent.</param>
        /// <returns></returns>
        Task<Nonce> Get(KeyId clientId, string nonceValue);
    }
}