using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _provider;
        private readonly RSACryptoServiceProvider _rsa;
        private readonly IDictionary<KeyId, SigningSettings> _interceptedSettingsDictionary;

        public CompositionTests() {
            _interceptedSettingsDictionary = new Dictionary<KeyId, SigningSettings>();
            _rsa = new RSACryptoServiceProvider();
            var services = new ServiceCollection()
                .AddHttpMessageSigning()
                .UseKeyId("unit-test-app")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("s3cr3t"))
                .UseOnRequestSignedEvent((message, signature, settings) => {
                    _interceptedSettingsDictionary[settings.KeyId] = settings;
                    return Task.CompletedTask;
                })
                .Services
                .AddHttpMessageSigning()
                .UseKeyId("unit-test-app-hmac")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("s3cr3t"))
                .UseHeaders((HeaderName) "hmacHeader")
                .UseOnRequestSignedEvent((message, signature, settings) => {
                    _interceptedSettingsDictionary[settings.KeyId] = settings;
                    return Task.CompletedTask;
                })
                .Services
                .AddHttpMessageSigning()
                .UseKeyId("unit-test-app-rsa")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning(_rsa))
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("s3cr3t"))
                .UseHeaders((HeaderName) "rsaHeader")
                .UseOnRequestSignedEvent((message, signature, settings) => {
                    _interceptedSettingsDictionary[settings.KeyId] = settings;
                    return Task.CompletedTask;
                })
                .Services;
            _provider = services.BuildServiceProvider();
        }

        public void Dispose() {
            _provider?.Dispose();
            _rsa?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IRequestSignerFactory))]
        public void CanResolveType(Type requestedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(requestedType);
        }

        [Theory]
        [InlineData("unit-test-app")]
        [InlineData("unit-test-app-hmac")]
        [InlineData("unit-test-app-rsa")]
        public void CanResolveRegisteredSigner(string keyId) {
            object actualSigner = null;
            Action act = () => actualSigner = _provider.GetRequiredService<IRequestSignerFactory>().CreateFor(keyId);
            act.Should().NotThrow();
            actualSigner.Should().NotBeNull();
            actualSigner.Should().BeAssignableTo<IRequestSigner>();
        }

        [Theory]
        [InlineData("something-not-registered")]
        public void CannotResolveUnregisteredKeyId(string keyId) {
            Action act = () => _provider.GetRequiredService<IRequestSignerFactory>().CreateFor(keyId);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task ResolvesCorrectSettingsWhenMultipleSignersAreRegistered() {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://dalion.eu") {
                Headers = {
                    {"hmacHeader", "hmac"},
                    {"rsaHeader", "rsa"}
                }
            };
            var factory = _provider.GetRequiredService<IRequestSignerFactory>();

            var signer1 = factory.CreateFor("unit-test-app");
            await signer1.Sign(request);

            _interceptedSettingsDictionary.Should().ContainKey("unit-test-app");
            _interceptedSettingsDictionary["unit-test-app"].Headers.Should().Equal(
                HeaderName.PredefinedHeaderNames.RequestTarget,
                HeaderName.PredefinedHeaderNames.Date);
            
            var signer2 = factory.CreateFor("unit-test-app-hmac");
            await signer2.Sign(request);

            _interceptedSettingsDictionary.Should().ContainKey("unit-test-app-hmac");
            _interceptedSettingsDictionary["unit-test-app-hmac"].Headers.Should().Equal(
                (HeaderName)"hmacHeader", 
                HeaderName.PredefinedHeaderNames.RequestTarget, 
                HeaderName.PredefinedHeaderNames.Date);
            
            var signer3 = factory.CreateFor("unit-test-app-rsa");
            await signer3.Sign(request);

            _interceptedSettingsDictionary.Should().ContainKey("unit-test-app-rsa");
            _interceptedSettingsDictionary["unit-test-app-rsa"].Headers.Should().Equal(
                (HeaderName)"rsaHeader", 
                HeaderName.PredefinedHeaderNames.RequestTarget, 
                HeaderName.PredefinedHeaderNames.Date);
        }

        [Fact]
        public void UponIncompleteRegistration_ThrowsValidationException() {
            using (var provider = new ServiceCollection()
                .AddHttpMessageSigning()
                .UseKeyId("unit-test-app")
                .Services
                .BuildServiceProvider()) {
                var factory = provider.GetRequiredService<IRequestSignerFactory>();
                Action act = () => factory.CreateFor("unit-test-app");
                act.Should().Throw<ValidationException>();
            }
        }
    }
}