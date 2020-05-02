using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class ExpiresHeaderAppenderTests {
        private readonly DateTimeOffset _timeOfComposing;

        public ExpiresHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
        }

        public class BuildStringToAppend : ExpiresHeaderAppenderTests {
            [Fact]
            public void WhenExpiresHasValue_ReturnsExpectedString() {
                var expires = TimeSpan.FromMinutes(10);
                var sut = new ExpiresHeaderAppender(_timeOfComposing, expires);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(expires): 1582540214";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void WhenExpiresHasNoValue_ReturnsEmptyString() {
                var sut = new ExpiresHeaderAppender(_timeOfComposing, null);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                actual.Should().Be(string.Empty);
            }
        }
    }
}