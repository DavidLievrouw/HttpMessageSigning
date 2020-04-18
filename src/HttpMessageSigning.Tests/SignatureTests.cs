using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SignatureTests {
        private readonly Signature _sut;

        public SignatureTests() {
            _sut = new Signature {
                Algorithm = "rsa-sha256",
                KeyId = "client1",
                String = "xyz123==",
                Created = new DateTimeOffset(2020, 2, 26, 13, 18, 12, TimeSpan.Zero),
                Expires = new DateTimeOffset(2020, 2, 26, 13, 23, 12, TimeSpan.Zero),
                Headers = new[] {(HeaderName) "h1", (HeaderName) "h2"}
            };
        }

        public class Validate : SignatureTests {
            [Fact]
            public void IsIValidatable() {
                Action act = () => ((IValidatable)_sut).Validate();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void GivenEmptyKeyId_ThrowsValidationException() {
                _sut.KeyId = KeyId.Empty;

                Action act = () => _sut.Validate();

                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptySignatureString_ThrowsValidationException(string nullOrEmpty) {
                _sut.String = nullOrEmpty;

                Action act = () => _sut.Validate();

                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }

        public class Clone : SignatureTests {
            [Fact]
            public void ReturnsNewInstanceWithExpectedValues() {
                var actual = _sut.Clone();
                actual.Should().NotBe(_sut);
                actual.Should().BeEquivalentTo(_sut);
            }
        }
    }
}