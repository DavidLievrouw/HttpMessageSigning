using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     A factory for <see cref="Client" /> instances.
    /// </summary>
    public static class ClientFactory {
        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class, with the specified options.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client.</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="configure">A delegate that allows configuring additional options for the new instance.</param>
        /// <returns>The newly created <see cref="Client" /> instance.</returns>
        public static Client Create(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, Action<ClientOptions> configure = null) {
            var options = new ClientOptions();
            configure?.Invoke(options);
            return new Client(id, name, signatureAlgorithm, options);
        }
    }
}