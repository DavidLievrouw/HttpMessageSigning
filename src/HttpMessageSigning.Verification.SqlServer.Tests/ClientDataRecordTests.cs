using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class ClientDataRecordTests {
        private readonly SharedSecretEncryptionKey _encryptionKey;
        private readonly string _unencryptedKey;

        public ClientDataRecordTests() {
            _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
            _unencryptedKey = "s3cr3t";
        }

        public class SetSignatureAlgorithm : ClientDataRecordTests {
            private readonly ClientDataRecord _sut;

            public SetSignatureAlgorithm() {
                _sut = new ClientDataRecord();
            }

            [Fact]
            public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                Action act = () => _sut.SetSignatureAlgorithm(signatureAlgorithm: null, _encryptionKey);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenUnsupportedAlgorithmType_ThrowsNotSupportedException() {
                var unsupported = new CustomSignatureAlgorithm("CUSTOM");
                Action act = () => _sut.SetSignatureAlgorithm(unsupported, _encryptionKey);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    Action act = () => _sut.SetSignatureAlgorithm(hmac, nullOrEmpty);
                    act.Should().NotThrow();
                }
            }

            [Fact]
            public void GivenHMACAlgorithm_SetsExpectedProperties() {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    _sut.SetSignatureAlgorithm(hmac, _encryptionKey);

                    _sut.SigType.Should().Be("HMAC");
                    _sut.SigHashAlgorithm.Should().Be(HashAlgorithmName.SHA384.Name);
                    _sut.IsSigParameterEncrypted.Should().Be(true);
                    _sut.SigParameter.Should().NotBe(_unencryptedKey);
                }
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotEncryptParameter(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    _sut.SetSignatureAlgorithm(hmac, nullOrEmpty);

                    _sut.SigParameter.Should().Be(_unencryptedKey);
                    _sut.IsSigParameterEncrypted.Should().BeFalse();
                }
            }

            [Fact]
            public void GivenRSAAlgorithm_ReturnsExpectedDataRecord() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    using (var rsaAlg = SignatureAlgorithm.CreateForVerification(rsa, HashAlgorithmName.SHA384)) {
                        _sut.SetSignatureAlgorithm(rsaAlg, _encryptionKey);

                        _sut.SigType.Should().Be("RSA");
                        _sut.SigParameter.Should().Be(rsa.ExportParameters(false).ToXml());
                        _sut.SigHashAlgorithm.Should().Be(HashAlgorithmName.SHA384.Name);
                        _sut.IsSigParameterEncrypted.Should().Be(false);
                    }
                }
            }

            [Fact]
            public void GivenECDsaAlgorithm_ReturnsExpectedDataRecord() {
                using (var ecdsa = ECDsa.Create()) {
                    using (var ecdsaAlg = SignatureAlgorithm.CreateForVerification(ecdsa, HashAlgorithmName.SHA384)) {
                        _sut.SetSignatureAlgorithm(ecdsaAlg, _encryptionKey);

                        _sut.SigType.Should().Be("ECDsa");
                        _sut.SigParameter.Should().Be(ecdsa.ExportParameters(false).ToXml());
                        _sut.SigHashAlgorithm.Should().Be(HashAlgorithmName.SHA384.Name);
                        _sut.IsSigParameterEncrypted.Should().Be(false);
                    }
                }
            }
        }

        public class GetSignatureAlgorithm : ClientDataRecordTests {
            private readonly ClientDataRecord _sut;
            private readonly string _encryptedKey;
            private readonly int? _recordVersion;

            public GetSignatureAlgorithm() {
                _encryptedKey = "VbB9IMM3ID9bc4l3gJnzlsZuYFWNqI6WUfRufiP1JHiwNcGRZWSn5Q82Imkn5luw";
                _recordVersion = 2;
                _sut = new ClientDataRecord {
                    SigType = "HMAC",
                    SigParameter = _encryptedKey,
                    SigHashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsSigParameterEncrypted = true
                };
            }

            [Fact]
            public void WhenTypeIsNull_ThrowsNotSupportedException() {
                _sut.SigType = null;
                Action act = () => _sut.GetSignatureAlgorithm(_encryptionKey, _recordVersion);
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void WhenTypeIsUnknown_ThrowsNotSupportedException() {
                _sut.SigType = "custom_unsupported";
                Action act = () => _sut.GetSignatureAlgorithm(_encryptionKey, _recordVersion);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                Action act = () => _sut.GetSignatureAlgorithm(nullOrEmpty, _recordVersion);
                act.Should().NotThrow();
            }

            [Theory]
            [InlineData("invalid_encryption_key")]
            [InlineData("the_big_secret")]
            public void GivenInvalidEncryptionKey_ThrowsSecurityException(string invalidKey) {
                Action act = () => _sut.GetSignatureAlgorithm(invalidKey, _recordVersion);
                act.Should().Throw<SecurityException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotDecryptParameter(string nullOrEmpty) {
                var sut = new ClientDataRecord {
                    SigType = "HMAC",
                    SigParameter = _unencryptedKey,
                    SigHashAlgorithm = HashAlgorithmName.MD5.Name
                };

                var actual = sut.GetSignatureAlgorithm(nullOrEmpty, _recordVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }

            [Fact]
            public void WhenParameterIsNotEncrypted_DoesNotDecryptParameter() {
                var sut = new ClientDataRecord {
                    SigType = "HMAC",
                    SigParameter = _unencryptedKey,
                    SigHashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsSigParameterEncrypted = false
                };

                var actual = sut.GetSignatureAlgorithm(_encryptionKey, _recordVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }

            [Fact]
            public void GivenRSADataRecord_ReturnsRSAAlgorithm() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicParameters = rsa.ExportParameters(false);
                    var sut = new ClientDataRecord {
                        SigType = "RSA",
                        SigParameter = publicParameters.ToXml(),
                        SigHashAlgorithm = HashAlgorithmName.MD5.Name,
                        IsSigParameterEncrypted = false
                    };

                    using (var actual = sut.GetSignatureAlgorithm(_encryptionKey, _recordVersion)) {
                        var expected = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.MD5, publicParameters);
                        actual.Should().BeAssignableTo<RSASignatureAlgorithm>();
                        actual.As<RSASignatureAlgorithm>().Should().BeEquivalentTo(expected);
                        actual.As<RSASignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(publicParameters.ToXml());
                    }
                }
            }

            [Fact]
            public void GivenECDsaDataRecord_ReturnsECDsaAlgorithm() {
                using (var ecdsa = ECDsa.Create()) {
                    var publicParameters = ecdsa.ExportParameters(false);
                    var sut = new ClientDataRecord {
                        SigType = "ECDsa",
                        SigParameter = publicParameters.ToXml(),
                        SigHashAlgorithm = HashAlgorithmName.MD5.Name,
                        IsSigParameterEncrypted = false
                    };

                    using (var actual = sut.GetSignatureAlgorithm(_encryptionKey, _recordVersion)) {
                        var expected = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.MD5, publicParameters);
                        actual.Should().BeAssignableTo<ECDsaSignatureAlgorithm>();
                        actual.As<ECDsaSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                        actual.As<ECDsaSignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(publicParameters.ToXml());
                    }
                }
            }

            [Fact]
            public void GivenHMACDataRecord_ReturnsHMACHashAlgorithm() {
                var sut = new ClientDataRecord {
                    SigType = "HMAC",
                    SigParameter = _encryptedKey,
                    SigHashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsSigParameterEncrypted = true
                };

                using (var actual = sut.GetSignatureAlgorithm(_encryptionKey, _recordVersion)) {
                    var expected = new HMACSignatureAlgorithm(_unencryptedKey, HashAlgorithmName.MD5);
                    actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                    actual.As<HMACSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                }
            }
        }
    }
}