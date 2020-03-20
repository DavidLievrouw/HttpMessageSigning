using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class SignatureSanitizerTests {
        private readonly IDefaultSignatureHeadersProvider _defaultSignatureHeadersProvider;
        private readonly SignatureSanitizer _sut;

        public SignatureSanitizerTests() {
            FakeFactory.Create(out _defaultSignatureHeadersProvider);
            _sut = new SignatureSanitizer(_defaultSignatureHeadersProvider);
        }

        public class Sanitize : SignatureSanitizerTests {
            private readonly Signature _signature;
            private readonly Client _client;

            public Sanitize() {
                _signature = new Signature {KeyId = "client1"};
                _client = new Client("client1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
            }

            [Fact]
            public void GivenNullSignature_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Sanitize(null, _client);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullClient_ThrowsArgumentException() {
                Func<Task> act = () => _sut.Sanitize(_signature, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task ReturnsAClone() {
                var actual = await _sut.Sanitize(_signature, _client);
                actual.Should().NotBe(_signature);
            }

            [Fact]
            public async Task WhenHeadersInSignatureAreNull_SetsDefaultHeadersForSignatureValidation() {
                _signature.Headers = null;

                var defaultHeaders = new[] {(HeaderName) "h10", (HeaderName) "h11"};
                A.CallTo(() => _defaultSignatureHeadersProvider.ProvideDefaultHeaders(_client.SignatureAlgorithm))
                    .Returns(defaultHeaders);

                var actual = await _sut.Sanitize(_signature, _client);

                actual.Headers.Should().BeEquivalentTo(new[] {(HeaderName) "h10", (HeaderName) "h11"});
            }

            [Fact]
            public async Task WhenHeadersInSignatureAreEmpty_SetsDefaultHeadersForSignatureValidation() {
                _signature.Headers = Array.Empty<HeaderName>();

                var defaultHeaders = new[] {(HeaderName) "h10", (HeaderName) "h11"};
                A.CallTo(() => _defaultSignatureHeadersProvider.ProvideDefaultHeaders(_client.SignatureAlgorithm))
                    .Returns(defaultHeaders);

                var actual = await _sut.Sanitize(_signature, _client);

                actual.Headers.Should().BeEquivalentTo(new[] {(HeaderName) "h10", (HeaderName) "h11"});
            }

            [Fact]
            public async Task WhenHeadersInSignatureAreSpecified_DoesNotChangeValue() {
                _signature.Headers = new[] {(HeaderName) "h1", (HeaderName) "h2"};

                var actual = await _sut.Sanitize(_signature, _client);

                actual.Headers.Should().BeEquivalentTo(new[] {(HeaderName) "h1", (HeaderName) "h2"});
            }
        }
    }
}