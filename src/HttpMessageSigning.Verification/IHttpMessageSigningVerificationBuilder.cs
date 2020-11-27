using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     A builder for configuring HTTP message signature verification.
    /// </summary>
    public interface IHttpMessageSigningVerificationBuilder {
        /// <summary>
        ///     Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }
        
        /// <summary>Register a <see cref="Client"/> in the <see cref="IClientStore"/>.</summary>
        /// <param name="client">The <see cref="Client" /> to register.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        IHttpMessageSigningVerificationBuilder UseClient(Client client);
        
        /// <summary>Configures HTTP message signature verification to use the specified <see cref="IClientStore"/>.</summary>
        /// <typeparam name="TClientStore">The type of the <see cref="IClientStore" /> that is to be used.</typeparam>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        IHttpMessageSigningVerificationBuilder UseClientStore<TClientStore>() where TClientStore : IClientStore;
        
        /// <summary>Configures HTTP message signature verification to use the specified <see cref="IClientStore"/>.</summary>
        /// <param name="clientStore">The <see cref="IClientStore" /> that is to be used.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        IHttpMessageSigningVerificationBuilder UseClientStore(IClientStore clientStore);
        
        /// <summary>Configures HTTP message signature verification to use the specified <see cref="IClientStore"/>.</summary>
        /// <param name="clientStoreFactory">The factory that creates the <see cref="IClientStore" /> that is to be used.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        IHttpMessageSigningVerificationBuilder UseClientStore(Func<IServiceProvider, IClientStore> clientStoreFactory);
    }
}