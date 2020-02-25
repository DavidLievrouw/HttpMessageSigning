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
            [Fact]
            public void WhenSignatureAlgorithmIsNotSupported_ThrowsNotSupportedException() {
                var secret = new HMACSecret("s3cr3t");
                Action act = () => _sut.Create(secret, HashAlgorithm.SHA256);
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void WhenSecretIsNull_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(null, HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenHMACSecret_CreatesExpectedAlgorithm() {
                var secret = new HMACSecret("s3cr3t");
                var actual = _sut.Create(secret, HashAlgorithm.SHA256);
                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
            }
        }
    }
}