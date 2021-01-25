using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class CompositionTests : MongoIntegrationTest, IDisposable {
        private readonly ServiceProvider _provider;
        private readonly string _connectionString;

        public CompositionTests(MongoSetup mongoSetup)
            : base(mongoSetup) {
            _connectionString = mongoSetup.MongoServerConnectionString.TrimEnd('/') + '/' + mongoSetup.DatabaseName;
            _provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseMongoDbClientStore(new MongoDbClientStoreSettings {
                    CollectionName = "clients",
                    ConnectionString = _connectionString,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseMongoDbNonceStore(new MongoDbNonceStoreSettings {
                    CollectionName = "nonces",
                    ConnectionString = _connectionString
                })
                .Services
                .BuildServiceProvider();
        }

        public void Dispose() {
            _provider?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IClientStore))]
        [InlineData(typeof(INonceStore))]
        public void CanResolveType(Type requestedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(requestedType);
        }

        [Theory]
        [InlineData(typeof(IClientStore), "CachingClientStore")]
        [InlineData(typeof(INonceStore), "CachingNonceStore")]
        public void CanResolveExactType(Type requestedType, string expectedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.GetType().Name.Should().Be(expectedType);
        }

        [Fact]
        public async Task RegistersClientsInMongo() {
            using (var provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseMongoDbClientStore(new MongoDbClientStoreSettings {
                    CollectionName = "clients",
                    ConnectionString = _connectionString,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseMongoDbNonceStore(new MongoDbNonceStoreSettings {
                    CollectionName = "nonces",
                    ConnectionString = _connectionString
                })
                .UseClient(Client.Create(
                    "e0e8dcd638334c409e1b88daf821d135",
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                    options => options.Claims = new [] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ))
                .Services
                .BuildServiceProvider()) {
                var clientStore = provider.GetRequiredService<IClientStore>();
                clientStore.GetType().Name.Should().Be("CachingClientStore");
                var registeredClient = await clientStore.Get("e0e8dcd638334c409e1b88daf821d135");
                registeredClient.Should().NotBeNull();
                registeredClient.Name.Should().Be("HttpMessageSigningSampleHMAC");
            }
        }
    }
}