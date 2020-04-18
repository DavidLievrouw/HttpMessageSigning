using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class CreatedHeaderAppenderTests {
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
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
                var sut = new CreatedHeaderAppender(algorithmName, _timeOfComposing);
                Action act = () => sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);
                act.Should().Throw<HttpMessageSigningException>();
            }
            
            [Fact]
            public void ReturnsExpectedString() {
                var sut = new CreatedHeaderAppender("hs2019", _timeOfComposing);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);

                var expected = "\n(created): 1582539614";
                actual.Should().Be(expected);
            }
        }
    }
}