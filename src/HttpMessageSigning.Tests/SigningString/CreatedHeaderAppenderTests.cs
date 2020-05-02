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
            [Fact]
            public void ReturnsExpectedString() {
                var sut = new CreatedHeaderAppender(_timeOfComposing);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);

                var expected = "\n(created): 1582539614";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void WhenTimeOfComposingHasNoValue_ReturnsEmptyString() {
                var sut = new CreatedHeaderAppender(null);
                
                var actual = sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created);

                actual.Should().Be(string.Empty);
            }
        }
    }
}