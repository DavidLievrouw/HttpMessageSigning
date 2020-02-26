using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class ExpiresHeaderAppenderTests {
        private readonly HttpRequestForSigning _request;
        private readonly ExpiresHeaderAppender _sut;
        private readonly DateTimeOffset _timeOfComposing;
        private readonly TimeSpan _expires;

        public ExpiresHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _expires = TimeSpan.FromMinutes(10);
            _request = new HttpRequestForSigning();
            _sut = new ExpiresHeaderAppender(_request, _timeOfComposing, _expires);
        }

        public class BuildStringToAppend : ExpiresHeaderAppenderTests {
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

                var expected = "\n(expires): 1582540214";
                actual.Should().Be(expected);
            }
        }
    }
}