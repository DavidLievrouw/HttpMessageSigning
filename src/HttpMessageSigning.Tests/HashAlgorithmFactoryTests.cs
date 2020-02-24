using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HashAlgorithmFactoryTests {
        private readonly HashAlgorithmFactory _sut;

        public HashAlgorithmFactoryTests() {
            _sut = new HashAlgorithmFactory();
        }

        public class Create : HashAlgorithmFactoryTests {
            [Fact]
            public void GivenSHA1_ReturnsExpectedAlgorithm() {
                var actual = _sut.Create(HashAlgorithm.SHA1);
                actual.Should().NotBeNull();
                actual.Should().BeAssignableTo<RealHashAlgorithm>();
                actual.Name.Should().Be("SHA-1");
            }

            [Fact]
            public void GivenSHA256_ReturnsExpectedAlgorithm() {
                var actual = _sut.Create(HashAlgorithm.SHA256);
                actual.Should().NotBeNull();
                actual.Should().BeAssignableTo<RealHashAlgorithm>();
                actual.Name.Should().Be("SHA-256");
            }

            [Fact]
            public void GivenSHA384_ReturnsExpectedAlgorithm() {
                var actual = _sut.Create(HashAlgorithm.SHA384);
                actual.Should().NotBeNull();
                actual.Should().BeAssignableTo<RealHashAlgorithm>();
                actual.Name.Should().Be("SHA-384");
            }

            [Fact]
            public void GivenSHA512_ReturnsExpectedAlgorithm() {
                var actual = _sut.Create(HashAlgorithm.SHA512);
                actual.Should().NotBeNull();
                actual.Should().BeAssignableTo<RealHashAlgorithm>();
                actual.Name.Should().Be("SHA-512");
            }

            [Fact]
            public void GivenInvalidValue_ThrowsArgumentOutOfRangeException() {
                Action act = () => _sut.Create((HashAlgorithm) (-999));
                act.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact]
            public void WhenRequestingNone_ThrowsArgumentOutOfRangeException() {
                Action act = () => _sut.Create(HashAlgorithm.None);
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }
    }
}