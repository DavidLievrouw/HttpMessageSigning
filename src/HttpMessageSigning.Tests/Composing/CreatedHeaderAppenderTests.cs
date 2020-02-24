using System;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Composing {
    public class CreatedHeaderAppenderTests {
        private readonly CreatedHeaderAppender _sut;
        private readonly ISystemClock _systemClock;

        public CreatedHeaderAppenderTests() {
            FakeFactory.Create(out _systemClock);
            _sut = new CreatedHeaderAppender(_systemClock);
        }

        public class BuildStringToAppend : CreatedHeaderAppenderTests {
            private readonly DateTimeOffset _now;

            public BuildStringToAppend() {
                _now = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                A.CallTo(() => _systemClock.UtcNow).Returns(_now.UtcDateTime);
            }

            [Fact]
            public void ReturnsExpectedString() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(created): 1582539614";
                actual.Should().Be(expected);
            }
        }
    }
}