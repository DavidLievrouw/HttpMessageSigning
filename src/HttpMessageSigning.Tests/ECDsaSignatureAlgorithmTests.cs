using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class ECDsaSignatureAlgorithmTests : IDisposable {
        private readonly ECDsa _ecdsa;
        private readonly ECParameters _privateKeyParams;
        private readonly ECParameters _publicKeyParams;
        private readonly ECDsaSignatureAlgorithm _signer;
        private readonly ECDsaSignatureAlgorithm _verifier;

        public ECDsaSignatureAlgorithmTests() {
            _ecdsa = ECDsa.Create();
            _publicKeyParams = _ecdsa.ExportParameters(false);
            _privateKeyParams = _ecdsa.ExportParameters(true);
            _signer = ECDsaSignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA1, _privateKeyParams);
            _verifier = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA1, _publicKeyParams);
        }

        public void Dispose() {
            _signer?.Dispose();
            _verifier?.Dispose();
            _ecdsa?.Dispose();
        }

        public class Name : ECDsaSignatureAlgorithmTests {
            [Fact]
            public void ReturnsHMAC() {
                _signer.Name.Should().Be("ECDsa");
                _verifier.Name.Should().Be("ECDsa");
            }
        }

        public class ComputeHash : ECDsaSignatureAlgorithmTests {
            [Fact]
            public void CreatesHash() {
                var sut = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA384, _ecdsa);
                var payload = "_abc_123_";
                var actual = sut.ComputeHash(payload);
                actual.Should().NotBeNull().And.NotBeEmpty();
            }

            [Fact]
            public void WhenHashAlgorithmIsNotSupported_ThrowsNotSupportedException() {
                var sut = new ECDsaSignatureAlgorithm(new HashAlgorithmName("unsupporteed"), _ecdsa);
                Action act = () => sut.ComputeHash("payload");
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void CanSignWithoutExportablePrivateKey() {
                using (var cert = new X509Certificate2("./dalion.local_ec.pfx", "CertP@ss123")) {
                    using (var sut = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA384, cert.GetECDsaPrivateKey())) {
                        var payload = "_abc_123_";
                        var actual = sut.ComputeHash(payload);
                        actual.Should().NotBeNull().And.NotBeEmpty();
                    }
                }
            }
        }

        public class VerifySignature : ECDsaSignatureAlgorithmTests {
            [Fact]
            public void CanVerifyValidSignature() {
                var payload = "_abc_123_";
                var signature = _signer.ComputeHash(payload);
                var actual = _verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
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
                var verifier = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA1, _publicKeyParams);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }

            [Fact]
            public void CanVerifyWithAlgorithmThatKnowsAboutThePrivateKey() {
                var payload = "_abc_123_";
                var signature = _signer.ComputeHash(payload);
                var verifier = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA1, _ecdsa);
                var actual = verifier.VerifySignature(payload, signature);
                actual.Should().BeTrue();
            }

            [Fact]
            public void CanVerifyValidSignatureFromNonExportableX509Certificate2() {
                using (var cert = new X509Certificate2("./dalion.local_ec.pfx", "CertP@ss123")) {
                    using (var signer = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA384, cert.GetECDsaPrivateKey())) {
                        var payload = "_abc_123_";
                        var signature = signer.ComputeHash(payload);

                        using (var verifier = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA384, cert.GetECDsaPublicKey())) {
                            var actual = verifier.VerifySignature(payload, signature);
                            actual.Should().BeTrue();
                        }
                    }
                }
            }
        }

        public class GetPublicKey : ECDsaSignatureAlgorithmTests {
            [Fact]
            public void ReturnsPublicKeyParameters() {
                var actual = _verifier.GetPublicKey();
                actual.Curve.Oid.FriendlyName.Should().BeEquivalentTo(_publicKeyParams.Curve.Oid.FriendlyName);
                actual.Q.X.Should().BeEquivalentTo(_publicKeyParams.Q.X);
                actual.Q.Y.Should().BeEquivalentTo(_publicKeyParams.Q.Y);
            }

            [Fact]
            public void DoesNotReturnPrivateKeyParameters() {
                var actual = _verifier.GetPublicKey();
                actual.D.Should().BeNull();
            }
        }
    }
}