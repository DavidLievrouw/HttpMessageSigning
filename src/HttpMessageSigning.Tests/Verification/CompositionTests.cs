using System;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _provider;
        private readonly RSACryptoServiceProvider _rsa;

        public CompositionTests() {
            _rsa = new RSACryptoServiceProvider();
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddHttpMessageSignatureVerification();
            _provider = services.BuildServiceProvider();
        }

        public void Dispose() {
            _provider?.Dispose();
            _rsa?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IRequestSignatureVerifier))]
        public void CanResolveType(Type requestedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(requestedType);
        }
    }
}