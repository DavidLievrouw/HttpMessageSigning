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
            [Fact]
            public void ReturnsExpectedString() {
                var sut = new ExpiresHeaderAppender(_timeOfComposing, _expires);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(expires): 1582540214";
                actual.Should().Be(expected);
            }
        }
    }
}