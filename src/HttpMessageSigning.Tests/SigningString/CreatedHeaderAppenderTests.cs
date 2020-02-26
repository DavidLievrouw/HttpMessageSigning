using System;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class CreatedHeaderAppenderTests {
        private readonly CreatedHeaderAppender _sut;
        private readonly SigningSettings _settings;
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _settings = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                KeyId = new KeyId("client1"),
                SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Expires,
                    new HeaderName("dalion_app_id")
                }
            };
            _sut = new CreatedHeaderAppender(_settings, _timeOfComposing);
        }

        public class BuildStringToAppend : CreatedHeaderAppenderTests {
            [Theory]
            [InlineData("rsa")]
            [InlineData("hmac")]
            [InlineData("ecdsa")]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public void WhenAlgorithmDoesNotAllowInclusionOfCreatedHeader_ThrowsHttpMessageSigningException(string algorithmName) {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm(algorithmName);
                Action act = () => _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);
                act.Should().Throw<HttpMessageSigningException>();
            }
            
            [Fact]
            public void ReturnsExpectedString() {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm("hs2019");
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);

                var expected = "\n(created): 1582539614";
                actual.Should().Be(expected);
            }
        }
    }
}