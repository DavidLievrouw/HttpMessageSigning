using System;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HMACSignatureAlgorithmTests {
        private readonly HMACSignatureAlgorithm _sut;

        public HMACSignatureAlgorithmTests() {
            _sut = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
        }

        public class Constructor : HMACSignatureAlgorithmTests {
            [Fact]
            public void GivenNullSecret_ThrowsArgumentNullException() {
                // ReSharper disable once ObjectCreationAsStatement
                Action act = () => new HMACSignatureAlgorithm(null, HashAlgorithmName.SHA256);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void AcceptsEmptySecret() {
                // ReSharper disable once ObjectCreationAsStatement
                Action act = () => new HMACSignatureAlgorithm("", HashAlgorithmName.SHA256);
                act.Should().NotThrow();
            }
        }

        public class Name : HMACSignatureAlgorithmTests {
            [Fact]
            public void ReturnsHMAC() {
                _sut.Name.Should().Be("HMAC");
            }
        }

        public class ComputeHash : HMACSignatureAlgorithmTests {
            [Fact]
            public void CreatesHash() {
                var sut = new HMACSignatureAlgorithm("", HashAlgorithmName.SHA384);
                var payload = "_abc_123_";
                var actual = sut.ComputeHash(payload);
                actual.Should().NotBeNull().And.NotBeEmpty();
            }

            [Fact]
            public void WhenHashAlgorithmIsNotSupported_ThrowsNotSupportedException() {
                var sut = new HMACSignatureAlgorithm("s3cr3t", new HashAlgorithmName("unsupporteed"));
                Action act = () => sut.ComputeHash("payload");
                act.Should().Throw<NotSupportedException>();
            }
        }
        
        public class VerifySignature : HMACSignatureAlgorithmTests {
            [Fact]
            public void GivenNullContentToSign_ThrowsArgumentNullException() {
                Action act = () => _sut.VerifySignature(null, new byte[] {0x01, 0x02});
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Fact]
            public void GivenNullSignature_ThrowsArgumentNullException() {
                Action act = () => _sut.VerifySignature("payload", null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CanUseEmptySecret() {
                var sut = new HMACSignatureAlgorithm("", HashAlgorithmName.SHA384);
                var payload = "_abc_123_";
                var signature = sut.ComputeHash(payload);
                var actual = sut.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }
            
            [Fact]
            public void CanVerifyValidSignature() {
                var payload = "_abc_123_";
                var signature = _sut.ComputeHash(payload);
                var actual = _sut.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }

            [Fact]
            public void VerificationFailsOnInvalidSignature() {
                var payload = "_abc_123_";
                var signature = _sut.ComputeHash(payload);
                signature[0]++; // Make it invalid
                var actual = _sut.VerifySignature(payload, signature);
                actual.Should().BeFalse();
            }
        }
    }
}