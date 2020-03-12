using System;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _provider;

        public CompositionTests() {
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddMongoDbClientStore(new MongoDbSettings {
                    CollectionName = "clients",
                    ConnectionString = "mongodb://localhost:27017/Auth"
                });
            _provider = services.BuildServiceProvider();
        }

        public void Dispose() {
            _provider?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IClientStore))]
        public void CanResolveType(Type requestedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(requestedType);
        }
        
        [Theory]
        [InlineData(typeof(IClientStore), typeof(MongoDbClientStore))]
        public void CanResolveExactType(Type requestedType, Type expectedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(expectedType);
        }
    }
}