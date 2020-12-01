using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _serviceProvider;

        public CompositionTests() {
            _serviceProvider = new ServiceCollection()
                .AddHttpMessageSignatureVerificationForOwin()
                .Services
                .BuildServiceProvider();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IRequestSignatureVerifier))]
        public void CanResolveType(Type requestedType) {
            var instance = _serviceProvider.GetRequiredService(requestedType);
            instance.Should().NotBeNull().And.BeAssignableTo(requestedType);
        }
    }
}