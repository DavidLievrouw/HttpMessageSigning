using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class SignatureParsingSuccessTests {
        public class Construction : SignatureParsingSuccessTests {
            [Fact]
            public void GivenNullSignature_ThrowsArgumentNullException() {
                Action act = () => new SignatureParsingSuccess(signature: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void AssignsSignature() {
                var signature = new Signature {String = "abc"};

                var sut = new SignatureParsingSuccess(signature);

                sut.Signature.Should().Be(signature);
            }
        }

        public class IsSuccess : SignatureParsingSuccessTests {
            [Fact]
            public void ReturnsTrue() {
                var sut = new SignatureParsingSuccess(new Signature {String = "abc"});

                sut.IsSuccess.Should().BeTrue();
            }
        }
    }
}