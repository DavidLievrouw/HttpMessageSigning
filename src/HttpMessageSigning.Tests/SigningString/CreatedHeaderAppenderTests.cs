using System;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class CreatedHeaderAppenderTests {
        private readonly CreatedHeaderAppender _sut;
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _sut = new CreatedHeaderAppender(_timeOfComposing);
        }

        public class BuildStringToAppend : CreatedHeaderAppenderTests {
            [Fact]
            public void ReturnsExpectedString() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(created): 1582539614";
                actual.Should().Be(expected);
            }
        }
    }
}