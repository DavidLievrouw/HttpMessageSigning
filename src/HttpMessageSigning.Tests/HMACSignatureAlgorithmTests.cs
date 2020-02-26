using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HMACSignatureAlgorithmTests {
        private readonly HMACSignatureAlgorithm _sut;

        public HMACSignatureAlgorithmTests() {
            _sut = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
        }

        public class Name : HMACSignatureAlgorithmTests {
            [Fact]
            public void ReturnsHMAC() {
                _sut.Name.Should().Be("HMAC");
            }
        }
        
        public class VerifySignature : HMACSignatureAlgorithmTests {
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