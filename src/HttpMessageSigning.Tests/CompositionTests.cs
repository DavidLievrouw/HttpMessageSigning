using System;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Dalion.HttpMessageSigning.Verification.MongoDb;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _serviceProvider;

        public CompositionTests() {
            var services = new ServiceCollection()
                .AddHttpMessageSigning(
                    new KeyId("client1"),
                    provider => new SigningSettings {
                        SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384)
                    })
                .AddHttpMessageSignatureVerification()
                .AddMongoDbClientStore(new MongoDbClientStoreSettings {
                    CollectionName = "testclients",
                    ConnectionString = "mongodb://unit-test:27017/HttpMessageSigningDb",
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(1)
                })
                .AddMongoDbNonceStore(new MongoDbNonceStoreSettings {
                    CollectionName = "testnonces",
                    ConnectionString = "mongodb://unit-test:27017/HttpMessageSigningDb"
                });
            _serviceProvider = services.BuildServiceProvider();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IRequestSignerFactory))]
        [InlineData(typeof(IRequestSignatureVerifier))]
        [InlineData(typeof(IRegisteredSignerSettingsStore))]
        public void CanResolveType(Type requestedType) {
            var instance = _serviceProvider.GetRequiredService(requestedType);
            instance.Should().NotBeNull().And.BeAssignableTo(requestedType);
        }
        
        [Theory]
        [InlineData(typeof(IClientStore), typeof(CachingMongoDbClientStore))]
        [InlineData(typeof(INonceStore), typeof(MongoDbNonceStore))]
        public void CanResolveExpectedType(Type requestedType, Type expectedType) {
            var instance = _serviceProvider.GetRequiredService(requestedType);
            instance.Should().NotBeNull().And.BeAssignableTo(expectedType);
        }
    }
}