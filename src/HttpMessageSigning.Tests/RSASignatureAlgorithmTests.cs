using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class RSASignatureAlgorithmTests {
        private readonly RSASignatureAlgorithm _sut;
        private readonly RSAParameters _privateKeyParams;
        private readonly RSAParameters _publicKeyParams;
        private readonly RSACryptoServiceProvider _rsa;

        public RSASignatureAlgorithmTests() {
            _rsa = new RSACryptoServiceProvider();
            _publicKeyParams = _rsa.ExportParameters(false);
            _privateKeyParams = _rsa.ExportParameters(true);
            _sut = new RSASignatureAlgorithm(HashAlgorithmName.SHA1, _publicKeyParams, _privateKeyParams);
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
                var verifier = new RSASignatureAlgorithm(HashAlgorithmName.SHA1, _publicKeyParams);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }
            
            [Fact]
            public void CanVerifyWithAlgorithmThatKnowsAboutThePrivateKey() {
                var payload = "_abc_123_";
                var signature = _sut.ComputeHash(payload);
                var verifier = new RSASignatureAlgorithm(HashAlgorithmName.SHA1, _rsa);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }
        }

        public class GetPublicKey : RSASignatureAlgorithmTests {
            [Fact]
            public void ReturnsPublicKeyParameters() {
                var actual = _sut.GetPublicKey();
                actual.D.Should().BeEquivalentTo(_publicKeyParams.D);
                actual.DP.Should().BeEquivalentTo(_publicKeyParams.DP);
                actual.DQ.Should().BeEquivalentTo(_publicKeyParams.DQ);
                actual.Exponent.Should().BeEquivalentTo(_publicKeyParams.Exponent);
                actual.InverseQ.Should().BeEquivalentTo(_publicKeyParams.InverseQ);
                actual.Modulus.Should().BeEquivalentTo(_publicKeyParams.Modulus);
                actual.P.Should().BeEquivalentTo(_publicKeyParams.P);
                actual.Q.Should().BeEquivalentTo(_publicKeyParams.Q);
            }
            [Fact]
            public void DoesNotReturnPrivateKeyParameters() {
                var actual = _sut.GetPublicKey();
                actual.D.Should().BeNull();
                actual.DP.Should().BeNull();
                actual.DQ.Should().BeNull();
                actual.InverseQ.Should().BeNull();
                actual.P.Should().BeNull();
                actual.Q.Should().BeNull();
            }
        }
    }
}