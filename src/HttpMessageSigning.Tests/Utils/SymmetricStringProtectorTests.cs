using System;
using FluentAssertions;
using Xunit;

#if NET472
using System.Security.Cryptography;
#endif

namespace Dalion.HttpMessageSigning.Utils {
    public class SymmetricStringProtectorTests {
        private readonly SymmetricStringProtector _sut;

        public SymmetricStringProtectorTests() {
            const string secret = "s3cr37";
            _sut = new SymmetricStringProtector(secret);
        }

        public class Construction : SymmetricStringProtectorTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptySecret_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => new SymmetricStringProtector(nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }
        }

        public class Protect : SymmetricStringProtectorTests {
            [Fact]
            public void GivenNullPlainText_Throws() {
                Action act = () => _sut.Protect(plainText: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void Encrypts() {
                var input = "abc123";
                var actual = _sut.Protect(input);
                actual.Should().NotBeNullOrEmpty().And.NotBe(input);
            }
        }

        public class Unprotect : SymmetricStringProtectorTests {
            [Fact]
            public void GivenNullCypher_Throws() {
                Action act = () => _sut.Unprotect(cipherText: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CanRoundTrip() {
                var input = "abc123";
                var encrypted = _sut.Protect(input);
                var decrypted = _sut.Unprotect(encrypted);
                decrypted.Should().Be(input);
            }

            [Fact]
            public void GivenInvalidCypher_ThrowsException() {
                var invalidCypher = "UVoVzETSbfe/OPtwk4wiKw==";
                Action act = () => _sut.Unprotect(invalidCypher);
#if NET472
                act.Should().Throw<CryptographicException>();
#else
                act.Should().Throw<ArgumentException>();
#endif
            }

            [Fact]
            public void GivenNonsenseCypher_ThrowsFormatException() {
                Action act = () => _sut.Unprotect("{nonsense}");
                act.Should().Throw<FormatException>();
            }
        }
    }
}