using System;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HashAlgorithmFactoryTests {
#if !NETFRAMEWORK
        [Theory]
        [InlineData("MD5", "MD5")]
        [InlineData("SHA1", "SHA1")]
        [InlineData("SHA256", "SHA256")]
        [InlineData("SHA384", "SHA384")]
        [InlineData("SHA512", "SHA512")]
        public void WhenAlgorithmIsSupported_ReturnsExpectedAlgorithm(string algorithm, string expectedType) {
            var actual = HashAlgorithmFactory.Create(new HashAlgorithmName(algorithm));
            actual.Should().NotBeNull();
            actual.Should().BeAssignableTo<HashAlgorithm>();
            actual.GetType().FullName.Should().Contain(expectedType + "+");
        }
#else
        [Theory]
        [InlineData("MD5", "MD5CryptoServiceProvider")]
        [InlineData("SHA1", "SHA1CryptoServiceProvider")]
        [InlineData("SHA256", "SHA256Managed")]
        [InlineData("SHA384", "SHA384Managed")]
        [InlineData("SHA512", "SHA512Managed")]
        public void WhenAlgorithmIsSupported_ReturnsExpectedAlgorithm(string algorithm, string expectedType) {
            var actual = HashAlgorithmFactory.Create(new HashAlgorithmName(algorithm));
            actual.Should().NotBeNull();
            actual.GetType().Name.Should().Be(expectedType);
        }
#endif
        
        [Fact]
        public void WhenAlgorithmIsEmpty_ThrowsArgumentException() {
            Action act = () => HashAlgorithmFactory.Create(new HashAlgorithmName());
            act.Should().Throw<ArgumentException>();
        }
        
        [Fact]
        public void WhenAlgorithmIsNotSupported_ThrowsNotSupportedException() {
            Action act = () => HashAlgorithmFactory.Create(new HashAlgorithmName("unsupported"));
            act.Should().Throw<NotSupportedException>();
        }
    }
}