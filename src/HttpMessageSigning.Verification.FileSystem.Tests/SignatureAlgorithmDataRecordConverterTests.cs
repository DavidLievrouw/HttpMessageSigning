using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Utils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class SignatureAlgorithmDataRecordConverterTests {
        private readonly FakeStringProtector _stringProtector;
        private readonly IStringProtectorFactory _stringProtectorFactory;
        private readonly SignatureAlgorithmDataRecordConverter _sut;

        public SignatureAlgorithmDataRecordConverterTests() {
            FakeFactory.Create(out _stringProtectorFactory);
            _sut = new SignatureAlgorithmDataRecordConverter(_stringProtectorFactory);

            _stringProtector = new FakeStringProtector();
            A.CallTo(() => _stringProtectorFactory.CreateSymmetric(A<string>._))
                .Returns(_stringProtector);
        }

        public class FromSignatureAlgorithm : SignatureAlgorithmDataRecordConverterTests {
            private readonly SharedSecretEncryptionKey _encryptionKey;
            private readonly string _unencryptedKey;

            public FromSignatureAlgorithm() {
                _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
                _unencryptedKey = "s3cr3t";
            }

            [Fact]
            public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                Action act = () => _sut.FromSignatureAlgorithm(null, _encryptionKey);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenUnsupportedAlgorithmType_ThrowsNotSupportedException() {
                var unsupported = new CustomSignatureAlgorithm("CUSTOM");
                Action act = () => _sut.FromSignatureAlgorithm(unsupported, _encryptionKey);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    Action act = () => _sut.FromSignatureAlgorithm(hmac, (SharedSecretEncryptionKey)nullOrEmpty);
                    act.Should().NotThrow();
                }
            }

            [Fact]
            public void GivenHMACAlgorithm_ReturnsExpectedDataRecord() {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    var actual = _sut.FromSignatureAlgorithm(hmac, _encryptionKey);
                    var encryptedKey = new FakeStringProtector().Protect(_unencryptedKey);
                    var expected = new SignatureAlgorithmDataRecord {
                        Type = "HMAC",
                        Hash = HashAlgorithmName.SHA384.Name,
                        Encrypted = true,
                        Param = $"<Secret>{encryptedKey}</Secret>"
                    };
                    actual.Should().BeEquivalentTo(expected);
                }
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotEncryptParameter(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    var actual = _sut.FromSignatureAlgorithm(hmac, (SharedSecretEncryptionKey)nullOrEmpty);
                    actual.Param.Should().Be($"<Secret>{_unencryptedKey}</Secret>");
                    actual.Encrypted.Should().BeFalse();
                }
            }

            [Fact]
            public void GivenRSAAlgorithm_ReturnsExpectedDataRecord() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    using (var rsaAlg = SignatureAlgorithm.CreateForVerification(rsa, HashAlgorithmName.SHA384)) {
                        var actual = _sut.FromSignatureAlgorithm(rsaAlg, _encryptionKey);
                        var expected = new SignatureAlgorithmDataRecord {
                            Type = "RSA",
                            Param = rsa.ExportParameters(false).ToXml(),
                            Hash = HashAlgorithmName.SHA384.Name,
                            Encrypted = false
                        };
                        actual.Should().BeEquivalentTo(expected);
                    }
                }
            }

            [Fact]
            public void GivenECDsaAlgorithm_ReturnsExpectedDataRecord() {
                using (var ecdsa = ECDsa.Create()) {
                    using (var ecdsaAlg = SignatureAlgorithm.CreateForVerification(ecdsa, HashAlgorithmName.SHA384)) {
                        var actual = _sut.FromSignatureAlgorithm(ecdsaAlg, _encryptionKey);
                        var expected = new SignatureAlgorithmDataRecord {
                            Type = "ECDsa",
                            Param = ecdsa.ExportParameters(false).ToXml(),
                            Hash = HashAlgorithmName.SHA384.Name,
                            Encrypted = false
                        };
                        actual.Should().BeEquivalentTo(expected);
                    }
                }
            }
        }

        public class ToSignatureAlgorithm : SignatureAlgorithmDataRecordConverterTests {
            private readonly SignatureAlgorithmDataRecord _dataRecord;
            private readonly int? _recordVersion;
            private readonly SharedSecretEncryptionKey _encryptionKey;
            private readonly string _unencryptedKey;
            private readonly string _encryptedKey;

            public ToSignatureAlgorithm() {
                _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
                _unencryptedKey = "s3cr3t";
                _encryptedKey = new FakeStringProtector().Protect(_unencryptedKey);
                _recordVersion = 2;
                _dataRecord = new SignatureAlgorithmDataRecord {
                    Type = "HMAC",
                    Param = $"<Secret>{_encryptedKey}</Secret>",
                    Hash = HashAlgorithmName.MD5.Name,
                    Encrypted = true
                };
            }

            [Fact]
            public void WhenDataRecordIsNull_ThrowsArgumentNullException() {
                Action act = () => _sut.ToSignatureAlgorithm(dataRecord: null, _encryptionKey, _recordVersion);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenTypeIsNull_ThrowsNotSupportedException() {
                _dataRecord.Type = null;
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey, _recordVersion);
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void WhenTypeIsUnknown_ThrowsNotSupportedException() {
                _dataRecord.Type = "custom_unsupported";
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey, _recordVersion);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, (SharedSecretEncryptionKey)nullOrEmpty, _recordVersion);
                act.Should().NotThrow();
            }

            [Theory]
            [InlineData("invalid_encryption_key")]
            [InlineData("the_big_secret")]
            public void GivenInvalidEncryptionKey_ThrowsSecurityException(string invalidKey) {
                _stringProtector.Throw(new FormatException("Invalid cypher."));

                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, (SharedSecretEncryptionKey)invalidKey, _recordVersion);
                act.Should().Throw<SecurityException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotDecryptParameter(string nullOrEmpty) {
                _dataRecord.Param = $"<Secret>{_unencryptedKey}</Secret>";

                var actual = _sut.ToSignatureAlgorithm(_dataRecord, (SharedSecretEncryptionKey)nullOrEmpty, _recordVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }

            [Fact]
            public void WhenParameterIsNotEncrypted_DoesNotDecryptParameter() {
                _dataRecord.Param = $"<Secret>{_unencryptedKey}</Secret>";
                _dataRecord.Encrypted = false;

                var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey, _recordVersion);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }

            [Fact]
            public void GivenRSADataRecord_ReturnsRSAAlgorithm() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicParameters = rsa.ExportParameters(includePrivateParameters: false);
                    _dataRecord.Type = "RSA";
                    _dataRecord.Param = publicParameters.ToXml();
                    _dataRecord.Hash = HashAlgorithmName.MD5.Name;
                    _dataRecord.Encrypted = false;

                    using (var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey, _recordVersion)) {
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
                    var publicParameters = ecdsa.ExportParameters(includePrivateParameters: false);
                    _dataRecord.Type = "ECDsa";
                    _dataRecord.Param = publicParameters.ToXml();
                    _dataRecord.Hash = HashAlgorithmName.MD5.Name;
                    _dataRecord.Encrypted = false;

                    using (var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey, _recordVersion)) {
                        var expected = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.MD5, publicParameters);
                        actual.Should().BeAssignableTo<ECDsaSignatureAlgorithm>();
                        actual.As<ECDsaSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                        actual.As<ECDsaSignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(publicParameters.ToXml());
                    }
                }
            }

            [Fact]
            public void GivenHMACDataRecord_ReturnsHMACDataRecord() {
                using (var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey, _recordVersion)) {
                    var expected = new HMACSignatureAlgorithm(_unencryptedKey, HashAlgorithmName.MD5);
                    actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                    actual.As<HMACSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                }
            }
        }
    }
}