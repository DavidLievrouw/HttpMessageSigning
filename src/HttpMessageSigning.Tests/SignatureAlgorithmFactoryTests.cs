using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SignatureAlgorithmFactoryTests {
        private readonly SignatureAlgorithmFactory _sut;

        public SignatureAlgorithmFactoryTests() {
            _sut = new SignatureAlgorithmFactory();
        }

        public class Create : SignatureAlgorithmFactoryTests {
            private readonly string _secret;

            public Create() {
                _secret = "s3cr3t";
            }

            [Fact]
            public void WhenSignatureAlgorithmIsNotHmac_ThrowsNotSupportedException() {
                Action act = () => _sut.Create(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, _secret);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void WhenSecretIsNullOrEmpty_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => _sut.Create(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }
            
            [Fact]
            public void CreatesAlgorithmWithExpectedKey() {
                var actual = _sut.Create(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, _secret);

                actual.Should().BeAssignableTo<RealSignatureAlgorithm>();
                var expectedSigningKey = new byte[] {0x73, 0x33, 0x63, 0x72, 0x33, 0x74};
                actual.As<RealSignatureAlgorithm>().Key.Should().BeEquivalentTo(expectedSigningKey);
                actual.As<RealSignatureAlgorithm>().Name.Should().BeEquivalentTo("HMACSHA256");
            }
        }
    }
}