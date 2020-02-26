using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class ExpiresHeaderAppenderTests {
        private readonly HttpRequestForSigning _request;
        private readonly ExpiresHeaderAppender _sut;
        private readonly DateTimeOffset _timeOfComposing;

        public ExpiresHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _request = new HttpRequestForSigning {
                Expires = TimeSpan.FromMinutes(5)
            };
            _sut = new ExpiresHeaderAppender(_request, _timeOfComposing);
        }

        public class BuildStringToAppend : ExpiresHeaderAppenderTests {
            [Fact]
            public void WhenExpiresHasNoValue_ReturnsEmptyString() {
                _request.Expires = null;
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                actual.Should().NotBeNull().And.BeEmpty();
            }
            
            [Theory]
            [InlineData("rsa")]
            [InlineData("hmac")]
            [InlineData("ecdsa")]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public void WhenAlgorithmDoesNotAllowInclusionOfExpiresHeader_ThrowsHttpMessageSigningException(string algorithmName) {
                _request.SignatureAlgorithmName = algorithmName;
                Action act = () => _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);
                act.Should().Throw<HttpMessageSigningException>();
            }
            
            [Fact]
            public void ReturnsExpectedString() {
                _request.SignatureAlgorithmName = "hs2019";
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(expires): 1582539914";
                actual.Should().Be(expected);
            }
        }
    }
}