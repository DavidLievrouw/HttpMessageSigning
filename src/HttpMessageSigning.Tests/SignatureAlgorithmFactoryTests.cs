using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SignatureAlgorithmFactoryTests {
        private readonly IHashAlgorithmFactory _hashAlgorithmFactory;
        private readonly SignatureAlgorithmFactory _sut;

        public SignatureAlgorithmFactoryTests() {
            FakeFactory.Create(out _hashAlgorithmFactory);
            _sut = new SignatureAlgorithmFactory(_hashAlgorithmFactory);
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
            public void CreatesExpectedAlgorithm() {
                var actual = _sut.Create(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, _secret);
                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
            }
        }
    }
}