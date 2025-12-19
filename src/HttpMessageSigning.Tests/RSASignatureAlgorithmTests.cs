using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class RSASignatureAlgorithmTests : IDisposable {
        private readonly RSAParameters _privateKeyParams;
        private readonly RSAParameters _publicKeyParams;
        private readonly RSACryptoServiceProvider _rsa;
        private readonly RSASignatureAlgorithm _signer;
        private readonly RSASignatureAlgorithm _verifier;

        public RSASignatureAlgorithmTests() {
            _rsa = new RSACryptoServiceProvider();
            _publicKeyParams = _rsa.ExportParameters(false);
            _privateKeyParams = _rsa.ExportParameters(true);
            _signer = RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA1, _privateKeyParams);
            _verifier = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA1, _publicKeyParams);
        }

        public void Dispose() {
            _signer?.Dispose();
            _verifier?.Dispose();
            _rsa?.Dispose();
        }

        public class Name : RSASignatureAlgorithmTests {
            [Fact]
            public void ReturnsHMAC() {
                _signer.Name.Should().Be("RSA");
                _verifier.Name.Should().Be("RSA");
            }
        }

        public class ComputeHash : RSASignatureAlgorithmTests {
            [Fact]
            public void CreatesHash() {
                var sut = new RSASignatureAlgorithm(HashAlgorithmName.SHA384, _rsa);
                var payload = "_abc_123_";
                var actual = sut.ComputeHash(payload);
                actual.Should().NotBeNull().And.NotBeEmpty();
            }

            [Fact]
            public void WhenHashAlgorithmIsNotSupported_ThrowsNotSupportedException() {
                var sut = new RSASignatureAlgorithm(new HashAlgorithmName("unsupporteed"), _rsa);
                Action act = () => sut.ComputeHash("payload");
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void CanSignWithoutExportablePrivateKey() {
#if NET10_0_OR_GREATER
                using (var cert = X509CertificateLoader.LoadPkcs12FromFile("./dalion.local.pfx", "CertP@ss123")) {
#else
                using (var cert = new X509Certificate2("./dalion.local.pfx", "CertP@ss123")) {
#endif
                    using (var sut = new RSASignatureAlgorithm(HashAlgorithmName.SHA384, cert.GetRSAPrivateKey())) {
                        var payload = "_abc_123_";
                        var actual = sut.ComputeHash(payload);
                        actual.Should().NotBeNull().And.NotBeEmpty();
                    }
                }
            }
        }

        public class VerifySignature : RSASignatureAlgorithmTests {
            [Fact]
            public void CanVerifyValidSignature() {
                var payload = "_abc_123_";
                var signature = _signer.ComputeHash(payload);
                var actual = _verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }

            [Fact]
            public void CanVerifyValidSignatureFromNonExportableX509Certificate2() {
#if NET10_0_OR_GREATER
                using (var cert = X509CertificateLoader.LoadPkcs12FromFile("./dalion.local.pfx", "CertP@ss123")) {
#else
                using (var cert = new X509Certificate2("./dalion.local.pfx", "CertP@ss123")) {
#endif
                    using (var signer = new RSASignatureAlgorithm(HashAlgorithmName.SHA384, cert.GetRSAPrivateKey())) {
                        var payload = "_abc_123_";
                        var signature = signer.ComputeHash(payload);

                        using (var verifier = new RSASignatureAlgorithm(HashAlgorithmName.SHA384, cert.GetRSAPublicKey())) {
                            var actual = verifier.VerifySignature(payload, signature);
                            actual.Should().BeTrue();
                        }
                    }
                }
            }

            [Fact]
            public void VerificationFailsOnInvalidSignature() {
                var payload = "_abc_123_";
                var signature = _signer.ComputeHash(payload);
                signature[0]++; // Make it invalid
                var actual = _verifier.VerifySignature(payload, signature);
                actual.Should().BeFalse();
            }

            [Fact]
            public void CanVerifyWithAlgorithmThatOnlyKnowsAboutThePublicKey() {
                var payload = "_abc_123_";
                var signature = _signer.ComputeHash(payload);
                var verifier = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA1, _publicKeyParams);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }

            [Fact]
            public void CanVerifyWithAlgorithmThatKnowsAboutThePrivateKey() {
                var payload = "_abc_123_";
                var signature = _signer.ComputeHash(payload);
                var verifier = new RSASignatureAlgorithm(HashAlgorithmName.SHA1, _rsa);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }
        }

        public class GetPublicKey : RSASignatureAlgorithmTests {
            [Fact]
            public void ReturnsPublicKeyParameters() {
                var actual = _verifier.GetPublicKey();
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
                var actual = _verifier.GetPublicKey();
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