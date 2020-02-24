using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class KeyedHashAlgorithmFactoryTests {
        private readonly KeyedHashAlgorithmFactory _sut;

        public KeyedHashAlgorithmFactoryTests() {
            _sut = new KeyedHashAlgorithmFactory();
        }

        public class Create : KeyedHashAlgorithmFactoryTests {
            [Fact]
            public void WhenSignatureAlgorithmIsNotHmac_ThrowsNotSupportedException() {
                Action act = () => _sut.Create(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, new byte[] {0x01, 0x02});
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void CreatesAlgorithmWithExpectedKey() {
                var signingKey = new byte[] {0x01, 0x02};

                var actual = _sut.Create(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, signingKey);

                actual.Should().BeAssignableTo<RealKeyedHashAlgorithm>();
                actual.As<RealKeyedHashAlgorithm>().Key.Should().BeEquivalentTo(signingKey);
            }
        }
    }
}