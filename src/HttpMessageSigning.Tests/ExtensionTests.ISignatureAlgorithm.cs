using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForISignatureAlgorithm : ExtensionTests {
            public class ShouldIncludeDateHeader : ForHttpMethod {
                [Fact]
                public void WhenSignatureAlgorithmIsNull_ThrowsArgumentNullException() {
                    Action act = () => Extensions.ShouldIncludeDateHeader(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Theory]
                [InlineData("rsa")]
                [InlineData("RSA")]
                [InlineData("hmac")]
                [InlineData("HMAC")]
                [InlineData("ecdsa")]
                [InlineData("ECDSA")]
                public void WhenHeaderShouldBeSpecified_ReturnsTrue(string algorithmName) {
                    var sut = new CustomSignatureAlgorithm(algorithmName);
                    var actual = sut.ShouldIncludeDateHeader();
                    actual.Should().BeTrue();
                }

                [Fact]
                public void WhenHeaderShouldNotBeSpecified_ReturnsFalse() {
                    var sut = new CustomSignatureAlgorithm("hs2019");
                    var actual = sut.ShouldIncludeDateHeader();
                    actual.Should().BeFalse();
                }
            }
            
            public class ShouldIncludeCreatedHeader : ForHttpMethod {
                [Fact]
                public void WhenSignatureAlgorithmIsNull_ThrowsArgumentNullException() {
                    Action act = () => Extensions.ShouldIncludeCreatedHeader(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Theory]
                [InlineData("rsa")]
                [InlineData("RSA")]
                [InlineData("hmac")]
                [InlineData("HMAC")]
                [InlineData("ecdsa")]
                [InlineData("ECDSA")]
                public void WhenHeaderShouldNotBeSpecified_ReturnsFalse(string algorithmName) {
                    var sut = new CustomSignatureAlgorithm(algorithmName);
                    var actual = sut.ShouldIncludeCreatedHeader();
                    actual.Should().BeFalse();
                }

                [Fact]
                public void WhenHeaderShouldBeSpecified_ReturnsTrue() {
                    var sut = new CustomSignatureAlgorithm("hs2019");
                    var actual = sut.ShouldIncludeCreatedHeader();
                    actual.Should().BeTrue();
                }
            }
            
            public class ShouldIncludeExpiresHeader : ForHttpMethod {
                [Fact]
                public void WhenSignatureAlgorithmIsNull_ThrowsArgumentNullException() {
                    Action act = () => Extensions.ShouldIncludeExpiresHeader(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Theory]
                [InlineData("rsa")]
                [InlineData("RSA")]
                [InlineData("hmac")]
                [InlineData("HMAC")]
                [InlineData("ecdsa")]
                [InlineData("ECDSA")]
                public void WhenHeaderShouldNotBeSpecified_ReturnsFalse(string algorithmName) {
                    var sut = new CustomSignatureAlgorithm(algorithmName);
                    var actual = sut.ShouldIncludeExpiresHeader();
                    actual.Should().BeFalse();
                }

                [Fact]
                public void WhenHeaderShouldBeSpecified_ReturnsTrue() {
                    var sut = new CustomSignatureAlgorithm("hs2019");
                    var actual = sut.ShouldIncludeExpiresHeader();
                    actual.Should().BeTrue();
                }
            }
        }
    }
}