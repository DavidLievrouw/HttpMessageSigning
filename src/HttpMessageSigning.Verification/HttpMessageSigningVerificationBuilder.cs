using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification {
    internal class HttpMessageSigningVerificationBuilder : IHttpMessageSigningVerificationBuilder {
        private IClientStore _clientStore;

        public HttpMessageSigningVerificationBuilder(IServiceCollection services) {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _clientStore = new InMemoryClientStore();
            Services.AddSingleton((InMemoryClientStore)_clientStore);
            Services.AddSingleton(prov => _clientStore);
        }

        public IServiceCollection Services { get; }
        
        public IHttpMessageSigningVerificationBuilder UseClient(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            _clientStore.Register(client)
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter().GetResult();
            
            return this;
        }

        public IHttpMessageSigningVerificationBuilder UseClientStore<TClientStore>() where TClientStore : IClientStore {
            return UseClientStore(provider => provider.GetRequiredService<TClientStore>());
        }

        public IHttpMessageSigningVerificationBuilder UseClientStore(IClientStore clientStore) {
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));
            
            return UseClientStore(provider => clientStore);
        }

        public IHttpMessageSigningVerificationBuilder UseClientStore(Func<IServiceProvider, IClientStore> clientStoreFactory) {
            if (clientStoreFactory == null) throw new ArgumentNullException(nameof(clientStoreFactory));
            
            Services.AddSingleton(provider => {
                _clientStore = clientStoreFactory(provider);
                return _clientStore;
            });

            return this;
        }
    }
}