using System;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.Validation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _serviceProvider;

        public CompositionTests() {
            var services = new ServiceCollection()
                .AddLogging()
                .AddHttpMessageSigning(new ClientKey {
                    Id = new KeyId("client1"),
                    Secret = new Secret("s3cr3t")
                })
                .AddHttpMessageSignatureValidation(new InMemoryClientStore());
            _serviceProvider = services.BuildServiceProvider();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IRequestSigner))]
        [InlineData(typeof(IRequestSignatureValidator))]
        public void CanResolveType(Type requestedType) {
            var instance = _serviceProvider.GetRequiredService(requestedType);
            instance.Should().NotBeNull().And.BeAssignableTo(requestedType);
        }
    }
}