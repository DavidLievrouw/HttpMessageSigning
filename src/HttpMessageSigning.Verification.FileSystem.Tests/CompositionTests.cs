using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _provider;
        private readonly string _clientsFilePath;
        private readonly string _noncesFilePath;

        public CompositionTests() {
            _clientsFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            _noncesFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            _provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseFileSystemClientStore(new FileSystemClientStoreSettings {
                    FilePath = _clientsFilePath,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseFileSystemNonceStore(new FileSystemNonceStoreSettings {
                    FilePath = _noncesFilePath
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
        public async Task RegistersClientsInStore() {
            using (var provider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseFileSystemClientStore(new FileSystemClientStoreSettings {
                    FilePath = _clientsFilePath,
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .UseFileSystemNonceStore(new FileSystemNonceStoreSettings {
                    FilePath = _noncesFilePath
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