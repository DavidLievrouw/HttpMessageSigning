using System;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class AuthorizationHeaderParamCreatorTests {
        private readonly ILogger<AuthorizationHeaderParamCreator> _logger;
        private readonly AuthorizationHeaderParamCreator _sut;

        public AuthorizationHeaderParamCreatorTests() {
            FakeFactory.Create(out _logger);
            _sut = new AuthorizationHeaderParamCreator(_logger);
        }

        public class CreateParam : AuthorizationHeaderParamCreatorTests {
            private readonly Signature _signature;

            public CreateParam() {
                _signature = new Signature {
                    KeyId = new KeyId("abc123"),
                    Algorithm = "hmac-sha512",
                    Created = new DateTimeOffset(2020, 2, 24, 13, 53, 12, TimeSpan.Zero),
                    Expires = new DateTimeOffset(2020, 2, 24, 13, 55, 12, TimeSpan.Zero),
                    Headers = new []{(HeaderName)"h1",(HeaderName)"h2"},
                    Nonce = "abc123",
                    String = "YmFzZTY0IGVuY29kZWQgc3RyaW5n"
                };
            }

            [Fact]
            public void GivenNullSignature_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateParam(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CreatesExpectedString() {
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",algorithm=\"hmac-sha512\",created=1582552392,expires=1582552512,headers=\"h1 h2\",nonce=\"abc123\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void WhenNonceIsNullOrEmpty_ReturnsStringWithoutNonce(string nullOrEmpty) {
                _signature.Nonce = nullOrEmpty;
                
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",algorithm=\"hmac-sha512\",created=1582552392,expires=1582552512,headers=\"h1 h2\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void WhenCreatedIsNotPresent_ReturnsStringWithoutCreated() {
                _signature.Created = null;
                
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",algorithm=\"hmac-sha512\",expires=1582552512,headers=\"h1 h2\",nonce=\"abc123\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }

            [Fact]
            public void WhenExpiresIsNotPresent_ReturnsStringWithoutExpires() {
                _signature.Expires = null;
                
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",algorithm=\"hmac-sha512\",created=1582552392,headers=\"h1 h2\",nonce=\"abc123\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void WhenAlgorithmIsNotPresent_ReturnsStringWithoutAlgorithm(string nullOrEmpty) {
                _signature.Algorithm = nullOrEmpty;
                
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",created=1582552392,expires=1582552512,headers=\"h1 h2\",nonce=\"abc123\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void WhenHeadersIsNotPresent_ReturnsStringWithoutHeaders() {
                _signature.Headers = null;
                
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",algorithm=\"hmac-sha512\",created=1582552392,expires=1582552512,nonce=\"abc123\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void WhenHeadersIsEmpty_ReturnsStringWithoutHeaders() {
                _signature.Headers = Array.Empty<HeaderName>();
                
                var actual = _sut.CreateParam(_signature);
                
                var expected = "keyId=\"abc123\",algorithm=\"hmac-sha512\",created=1582552392,expires=1582552512,nonce=\"abc123\",signature=\"YmFzZTY0IGVuY29kZWQgc3RyaW5n\"";
                actual.Should().Be(expected);
            }
        }
    }
}