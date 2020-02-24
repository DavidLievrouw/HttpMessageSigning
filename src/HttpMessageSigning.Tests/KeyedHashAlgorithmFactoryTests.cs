using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class KeyedHashAlgorithmFactoryTests {
        private readonly KeyedHashAlgorithmFactory _sut;

        public KeyedHashAlgorithmFactoryTests() {
            _sut = new KeyedHashAlgorithmFactory();
        }

        public class Create : KeyedHashAlgorithmFactoryTests {
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

                actual.Should().BeAssignableTo<RealKeyedHashAlgorithm>();
                var expectedSigningKey = new byte[] {0x73, 0x33, 0x63, 0x72, 0x33, 0x74};
                actual.As<RealKeyedHashAlgorithm>().Key.Should().BeEquivalentTo(expectedSigningKey);
                actual.As<RealKeyedHashAlgorithm>().Name.Should().BeEquivalentTo("HMACSHA256");
            }
        }
    }
}