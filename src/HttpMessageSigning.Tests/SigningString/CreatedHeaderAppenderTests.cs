using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class CreatedHeaderAppenderTests {
        private readonly CreatedHeaderAppender _sut;
        private readonly DateTimeOffset _timeOfComposing;
        private readonly HttpRequestForSigning _request;

        public CreatedHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _request = new HttpRequestForSigning();
            _sut = new CreatedHeaderAppender(_request, _timeOfComposing);
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
                _request.SignatureAlgorithmName = algorithmName;
                Action act = () => _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);
                act.Should().Throw<HttpMessageSigningException>();
            }
            
            [Fact]
            public void ReturnsExpectedString() {
                _request.SignatureAlgorithmName = "hs2019";
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);

                var expected = "\n(created): 1582539614";
                actual.Should().Be(expected);
            }
        }
    }
}