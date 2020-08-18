using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class SignatureParsingFailureTests {
        public class Construction : SignatureParsingFailureTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyDescription_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => new SignatureParsingFailure(nullOrEmpty, new Exception("test"));
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void AcceptsNullFailure() {
                Action act = () => new SignatureParsingFailure("abc", failure: null);
                act.Should().NotThrow();
            }

            [Fact]
            public void AssignsDescriptionAndFailure() {
                var failure = new Exception("test");

                var sut = new SignatureParsingFailure("abc", failure);

                sut.Description.Should().Be("abc");
                sut.Failure.Should().Be(failure);
            }
        }

        public class IsSuccess : SignatureParsingFailureTests {
            [Fact]
            public void ReturnsFalse() {
                var sut = new SignatureParsingFailure("abc");

                sut.IsSuccess.Should().BeFalse();
            }
        }
    }
}