using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SignatureAlgorithmDataRecordTests {
        private readonly SharedSecretEncryptionKey _encryptionKey;
        private readonly string _unencryptedKey;

        public SignatureAlgorithmDataRecordTests() {
            _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
            _unencryptedKey = "s3cr3t";
        }

        public class FromSignatureAlgorithm : SignatureAlgorithmDataRecordTests {
            [Fact]
            public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                Action act = () => SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(null, _encryptionKey);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenUnsupportedAlgorithmType_ThrowsNotSupportedException() {
                var unsupported = new CustomSignatureAlgorithm("CUSTOM");
                Action act = () => SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(unsupported, _encryptionKey);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    Action act = () => SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(hmac, nullOrEmpty);
                    act.Should().NotThrow();
                }
            }
            
            [Fact]
            public void GivenHMACAlgorithm_ReturnsExpectedDataRecord() {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    var actual = SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(hmac, _encryptionKey);
                    var expected = new SignatureAlgorithmDataRecordV2 {
                        Type = "HMAC",
                        HashAlgorithm = HashAlgorithmName.SHA384.Name,
                        IsParameterEncrypted = true
                    };
                    actual.Should().BeEquivalentTo(expected, opts => opts.Excluding(_ => _.Parameter));
                    actual.Parameter.Should().NotBe(_unencryptedKey);
                }
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotEncryptParameter(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    var actual = SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(hmac, nullOrEmpty);
                    actual.Parameter.Should().Be(_unencryptedKey);
                    actual.IsParameterEncrypted.Should().BeFalse();
                }
            }
            
            [Fact]
            public void GivenRSAAlgorithm_ReturnsExpectedDataRecord() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    using (var rsaAlg = SignatureAlgorithm.CreateForVerification(rsa, HashAlgorithmName.SHA384)) {
                        var actual = SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(rsaAlg, _encryptionKey);
                        var expected = new SignatureAlgorithmDataRecordV2 {
                            Type = "RSA",
                            Parameter = rsa.ExportParameters(false).ToXml(),
                            HashAlgorithm = HashAlgorithmName.SHA384.Name,
                            IsParameterEncrypted = false
                        };
                        actual.Should().BeEquivalentTo(expected);
                    }
                }
            }

            [Fact]
            public void GivenECDsaAlgorithm_ReturnsExpectedDataRecord() {
                using (var ecdsa = ECDsa.Create()) {
                    using (var ecdsaAlg = SignatureAlgorithm.CreateForVerification(ecdsa, HashAlgorithmName.SHA384)) {
                        var actual = SignatureAlgorithmDataRecordV2.FromSignatureAlgorithm(ecdsaAlg, _encryptionKey);
                        var expected = new SignatureAlgorithmDataRecordV2 {
                            Type = "ECDsa",
                            Parameter = ecdsa.ExportParameters(false).ToXml(),
                            HashAlgorithm = HashAlgorithmName.SHA384.Name,
                            IsParameterEncrypted = false
                        };
                        actual.Should().BeEquivalentTo(expected);
                    }
                }
            }
        }

        public class ToSignatureAlgorithm : SignatureAlgorithmDataRecordTests {
            private readonly SignatureAlgorithmDataRecordV2 _sut;
            private readonly string _encryptedKey;
            private readonly int? _recordVersion;

            public ToSignatureAlgorithm() {
                _encryptedKey = "VbB9IMM3ID9bc4l3gJnzlsZuYFWNqI6WUfRufiP1JHiwNcGRZWSn5Q82Imkn5luw";
                _recordVersion = 2;
                _sut = new SignatureAlgorithmDataRecordV2 {
                    Type = "HMAC",
                    Parameter = _encryptedKey,
                    HashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsParameterEncrypted = true
                };
            }

            [Fact]
            public void WhenTypeIsNull_ThrowsNotSupportedException() {
                _sut.Type = null;
                Action act = () => _sut.ToSignatureAlgorithm(_encryptionKey, _recordVersion);
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void WhenTypeIsUnknown_ThrowsNotSupportedException() {
                _sut.Type = "custom_unsupported";
                Action act = () => _sut.ToSignatureAlgorithm(_encryptionKey, _recordVersion);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                Action act = () => _sut.ToSignatureAlgorithm(nullOrEmpty, _recordVersion);
                act.Should().NotThrow();
            }

            [Theory]
            [InlineData("invalid_encryption_key")]
            [InlineData("the_big_secret")]
            public void GivenInvalidEncryptionKey_ThrowsSecurityException(string invalidKey) {
                Action act = () => _sut.ToSignatureAlgorithm(invalidKey, _recordVersion);
                act.Should().Throw<SecurityException>();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotDecryptParameter(string nullOrEmpty) {
                var sut = new SignatureAlgorithmDataRecordV2 {
                    Type = "HMAC",
                    Parameter = _unencryptedKey,
                    HashAlgorithm = HashAlgorithmName.MD5.Name
                };
                
                var actual = sut.ToSignatureAlgorithm(nullOrEmpty, _recordVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }
            
            [Fact]
            public void WhenParameterIsNotEncrypted_DoesNotDecryptParameter() {
                var sut = new SignatureAlgorithmDataRecordV2 {
                    Type = "HMAC",
                    Parameter = _unencryptedKey,
                    HashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsParameterEncrypted = false
                };
                
                var actual = sut.ToSignatureAlgorithm(_encryptionKey, _recordVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData(0)]
            [InlineData(1)]
            public void GivenLegacyRecordVersion_DoesNotDecryptParameter(int? legacyVersion) {
                var sut = new SignatureAlgorithmDataRecordV2 {
                    Type = "HMAC",
                    Parameter = _unencryptedKey,
                    HashAlgorithm = HashAlgorithmName.MD5.Name
                };
                
                var actual = sut.ToSignatureAlgorithm(_encryptionKey, legacyVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }
            
            [Fact]
            public void GivenRSADataRecord_ReturnsRSAAlgorithm() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicParameters = rsa.ExportParameters(false);
                    var sut = new SignatureAlgorithmDataRecordV2 {
                        Type = "RSA",
                        Parameter = publicParameters.ToXml(),
                        HashAlgorithm = HashAlgorithmName.MD5.Name,
                        IsParameterEncrypted = false
                    };

                    using (var actual = sut.ToSignatureAlgorithm(_encryptionKey, _recordVersion)) {
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
                    var sut = new SignatureAlgorithmDataRecordV2 {
                        Type = "ECDsa",
                        Parameter = publicParameters.ToXml(),
                        HashAlgorithm = HashAlgorithmName.MD5.Name,
                        IsParameterEncrypted = false
                    };

                    using (var actual = sut.ToSignatureAlgorithm(_encryptionKey, _recordVersion)) {
                        var expected = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.MD5, publicParameters);
                        actual.Should().BeAssignableTo<ECDsaSignatureAlgorithm>();
                        actual.As<ECDsaSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                        actual.As<ECDsaSignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(publicParameters.ToXml());
                    }
                }
            }

            [Fact]
            public void GivenHMACDataRecord_ReturnsHMACDataRecord() {
                var sut = new SignatureAlgorithmDataRecordV2 {
                    Type = "HMAC",
                    Parameter = _encryptedKey,
                    HashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsParameterEncrypted = true
                };

                using (var actual = sut.ToSignatureAlgorithm(_encryptionKey, _recordVersion)) {
                    var expected = new HMACSignatureAlgorithm(_unencryptedKey, HashAlgorithmName.MD5);
                    actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                    actual.As<HMACSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                }
            }
        }
    }
}