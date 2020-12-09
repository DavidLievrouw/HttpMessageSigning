using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _provider;
        private readonly string _connectionString;

        public CompositionTests() {
            _connectionString = "ToDo";
            _provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseSqlServerClientStore(new SqlServerClientStoreSettings {
                    TableName = "clients",
                    ConnectionString = _connectionString,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseSqlServerNonceStore(new SqlServerNonceStoreSettings {
                    TableName = "nonces",
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
        [InlineData(typeof(IClientStore), typeof(CachingSqlServerClientStore))]
        [InlineData(typeof(INonceStore), typeof(CachingSqlServerNonceStore))]
        public void CanResolveExactType(Type requestedType, Type expectedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(expectedType);
        }

        [Fact]
        public async Task RegistersClientsInStore() {
            using (var provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseSqlServerClientStore(new SqlServerClientStoreSettings {
                    TableName = "clients",
                    ConnectionString = _connectionString,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseSqlServerNonceStore(new SqlServerNonceStoreSettings {
                    TableName = "nonces",
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
                clientStore.Should().BeAssignableTo<CachingSqlServerClientStore>();
                var registeredClient = await clientStore.Get("e0e8dcd638334c409e1b88daf821d135");
                registeredClient.Should().NotBeNull();
                registeredClient.Name.Should().Be("HttpMessageSigningSampleHMAC");
            }
        }
    }
}