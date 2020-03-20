using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class NonceAppenderTests {
        private readonly NonceAppender _sut;

        public NonceAppenderTests() {
            _sut = new NonceAppender();
        }

        public class BuildStringToAppend : NonceAppenderTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyNonce_ReturnsEmptyString(string nullOrEmpty) {
                var actual = _sut.BuildStringToAppend(nullOrEmpty);
                actual.Should().Be(string.Empty);
            }

            [Fact]
            public void GivenSomeNonce_ReturnsExpectedString() {
                var actual = _sut.BuildStringToAppend("abc123");
                actual.Should().Be("\nnonce: abc123");
            }
        }
    }
}