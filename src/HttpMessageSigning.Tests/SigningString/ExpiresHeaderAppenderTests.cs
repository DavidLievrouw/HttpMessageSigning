using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class ExpiresHeaderAppenderTests {
        private readonly DateTimeOffset _timeOfComposing;
        private readonly TimeSpan _expires;

        public ExpiresHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _expires = TimeSpan.FromMinutes(10);
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
                var sut = new ExpiresHeaderAppender(algorithmName, _timeOfComposing, _expires);
                Action act = () => sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);
                act.Should().Throw<HttpMessageSigningException>();
            }
            
            [Fact]
            public void ReturnsExpectedString() {
                var sut = new ExpiresHeaderAppender("hs2019", _timeOfComposing, _expires);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(expires): 1582540214";
                actual.Should().Be(expected);
            }
        }
    }
}