using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class CompositionTests : SqlServerIntegrationTest {
        private readonly ServiceProvider _provider;
        private readonly string _connectionString;

        public CompositionTests(SqlServerFixture fixture)
            : base(fixture) {
            _connectionString = fixture.SqlServerConfig.GetConnectionStringForTestDatabase();
            _provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseSqlServerClientStore(new SqlServerClientStoreSettings {
                    ClientsTableName = "dbo.Clients",
                    ConnectionString = _connectionString,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseSqlServerNonceStore(new SqlServerNonceStoreSettings {
                    NonceTableName = "dbo.Nonces",
                    ConnectionString = _connectionString
                })
                .Services
                .BuildServiceProvider();
        }

        public override void Dispose() {
            _provider?.Dispose();
            base.Dispose();
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
        public async Task RegistersClientsInStore() {
            using (var provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseSqlServerClientStore(new SqlServerClientStoreSettings {
                    ClientsTableName = "clients",
                    ConnectionString = _connectionString,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseSqlServerNonceStore(new SqlServerNonceStoreSettings {
                    NonceTableName = "nonces",
                    ConnectionString = _connectionString
                })
                .UseClient(Client.Create(
                    (KeyId)"e0e8dcd638334c409e1b88daf821d135",
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
                var registeredClient = await clientStore.Get((KeyId)"e0e8dcd638334c409e1b88daf821d135");
                registeredClient.Should().NotBeNull();
                registeredClient.Name.Should().Be("HttpMessageSigningSampleHMAC");
            }
        }
    }
}