using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class RSASignatureAlgorithmTests {
        private readonly RSASignatureAlgorithm _sut;
        private readonly RSAParameters _privateKeyParams;
        private readonly RSAParameters _publicKeyParams;

        public RSASignatureAlgorithmTests() {
            var rsa = new RSACryptoServiceProvider();
            _publicKeyParams = rsa.ExportParameters(false);
            _privateKeyParams = rsa.ExportParameters(true);
            _sut = new RSASignatureAlgorithm(HashAlgorithm.SHA1, _publicKeyParams, _privateKeyParams);
        }

        public class Name : RSASignatureAlgorithmTests {
            [Fact]
            public void ReturnsHMAC() {
                _sut.Name.Should().Be("RSA");
            }
        }

        public class VerifySignature : RSASignatureAlgorithmTests {
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

            [Fact]
            public void CanVerifyWithAlgorithmThatOnlyKnowsAboutThePublicKey() {
                var payload = "_abc_123_";
                var signature = _sut.ComputeHash(payload);
                var verifier = new RSASignatureAlgorithm(HashAlgorithm.SHA1, _publicKeyParams);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }
        }
    }
}