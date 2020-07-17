using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class ValidationErrorTests {
        public class Ctor : ValidationErrorTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyPropertyName_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => new ValidationError(nullOrEmpty, "msg");
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyMessage_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => new ValidationError("prop", nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void AssignsProperties() {
                var actual = new ValidationError("prop", "msg");

                actual.PropertyName.Should().Be("prop");
                actual.Message.Should().Be("msg");
            }
        }
    }
}