using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification {
    internal class HttpMessageSigningVerificationBuilder : IHttpMessageSigningVerificationBuilder {
        private IClientStore _clientStore;

        public HttpMessageSigningVerificationBuilder(IServiceCollection services) {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            ClientFactories = new List<Func<IServiceProvider, Client>>();

            _clientStore = new InMemoryClientStore();
            UseClientStore(prov => _clientStore);
        }

        internal IList<Func<IServiceProvider, Client>> ClientFactories { get; }

        public IServiceCollection Services { get; }

        public IHttpMessageSigningVerificationBuilder UseClient(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            ClientFactories.Add(prov => client);

            return this;
        }

        public IHttpMessageSigningVerificationBuilder UseClient(Func<IServiceProvider, Client> clientFactory) {
            if (clientFactory == null) throw new ArgumentNullException(nameof(clientFactory));

            ClientFactories.Add(clientFactory);

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
                foreach (var clientFactory in ClientFactories) {
                    var client = clientFactory?.Invoke(provider);
                    if (client != null) {
                        _clientStore
                            .Register(client)
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                return _clientStore;
            });

            return this;
        }

        public IHttpMessageSigningVerificationBuilder UseClaimsPrincipalFactory<TClaimsPrincipalFactory>() where TClaimsPrincipalFactory : IClaimsPrincipalFactory {
            return UseClaimsPrincipalFactory(provider => provider.GetRequiredService<TClaimsPrincipalFactory>());
        }

        public IHttpMessageSigningVerificationBuilder UseClaimsPrincipalFactory(IClaimsPrincipalFactory claimsPrincipalFactory) {
            if (claimsPrincipalFactory == null) throw new ArgumentNullException(nameof(claimsPrincipalFactory));

            return UseClaimsPrincipalFactory(provider => claimsPrincipalFactory);
        }

        public IHttpMessageSigningVerificationBuilder UseClaimsPrincipalFactory(Func<IServiceProvider, IClaimsPrincipalFactory> claimsPrincipalFactory) {
            if (claimsPrincipalFactory == null) throw new ArgumentNullException(nameof(claimsPrincipalFactory));

            Services.AddSingleton(claimsPrincipalFactory);

            return this;
        }
    }
}