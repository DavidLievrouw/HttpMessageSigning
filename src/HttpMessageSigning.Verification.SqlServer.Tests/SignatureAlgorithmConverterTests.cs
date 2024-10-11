using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Utils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SignatureAlgorithmConverterTests {
        private readonly IStringProtectorFactory _stringProtectorFactory;
        private readonly SignatureAlgorithmConverter _sut;
        private readonly FakeStringProtector _stringProtector;

        public SignatureAlgorithmConverterTests() {
            FakeFactory.Create(out _stringProtectorFactory); 
            _sut = new SignatureAlgorithmConverter(_stringProtectorFactory);

            _stringProtector = new FakeStringProtector();
            A.CallTo(() => _stringProtectorFactory.CreateSymmetric(A<string>._))
                .Returns(_stringProtector);
        }

        public class SetSignatureAlgorithm : SignatureAlgorithmConverterTests {
            private readonly SharedSecretEncryptionKey _encryptionKey;
            private readonly string _unencryptedKey;
            private readonly ClientDataRecord _dataRecord;

            public SetSignatureAlgorithm() {
                _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
                _unencryptedKey = "s3cr3t";
                _dataRecord = new ClientDataRecord {
                    V = ClientDataRecord.GetV()
                };
            }

            [Fact]
            public void GivenNullDataRecord_ThrowsArgumentNullException() {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    Action act = () => _sut.SetSignatureAlgorithm(null, hmac, _encryptionKey);
                    act.Should().Throw<ArgumentNullException>();
                }
            }

            [Fact]
            public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                Action act = () => _sut.SetSignatureAlgorithm(_dataRecord, null, _encryptionKey);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenUnsupportedAlgorithmType_ThrowsNotSupportedException() {
                var unsupported = new CustomSignatureAlgorithm("CUSTOM");
                Action act = () => _sut.SetSignatureAlgorithm(_dataRecord, unsupported, _encryptionKey);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    Action act = () => _sut.SetSignatureAlgorithm(_dataRecord, hmac, (SharedSecretEncryptionKey)nullOrEmpty);
                    act.Should().NotThrow();
                }
            }
            
            [Fact]
            public void GivenHMACAlgorithm_ReturnsExpectedDataRecord() {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    _sut.SetSignatureAlgorithm(_dataRecord, hmac, _encryptionKey);

                    _dataRecord.SigType.Should().Be("HMAC");
                    _dataRecord.SigHashAlgorithm.Should().Be(HashAlgorithmName.SHA384.Name);
                    _dataRecord.IsSigParameterEncrypted.Should().BeTrue();
                    var encryptedKey = new FakeStringProtector().Protect(_unencryptedKey);
                    _dataRecord.SigParameter.Should().Be(encryptedKey);
                }
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotEncryptParameter(string nullOrEmpty) {
                using (var hmac = SignatureAlgorithm.CreateForVerification(_unencryptedKey, HashAlgorithmName.SHA384)) {
                    _sut.SetSignatureAlgorithm(_dataRecord, hmac, (SharedSecretEncryptionKey)nullOrEmpty);
                    _dataRecord.SigParameter.Should().Be(_unencryptedKey);
                    _dataRecord.IsSigParameterEncrypted.Should().BeFalse();
                }
            }
            
            [Fact]
            public void GivenRSAAlgorithm_ReturnsExpectedDataRecord() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    using (var rsaAlg = SignatureAlgorithm.CreateForVerification(rsa, HashAlgorithmName.SHA384)) {
                        _sut.SetSignatureAlgorithm(_dataRecord, rsaAlg, _encryptionKey);
                        
                        _dataRecord.SigType.Should().Be("RSA");
                        _dataRecord.SigHashAlgorithm.Should().Be(HashAlgorithmName.SHA384.Name);
                        _dataRecord.IsSigParameterEncrypted.Should().BeFalse();
                        _dataRecord.SigParameter.Should().Be(rsa.ExportParameters(false).ToXml());
                    }
                }
            }

            [Fact]
            public void GivenECDsaAlgorithm_ReturnsExpectedDataRecord() {
                using (var ecdsa = ECDsa.Create()) {
                    using (var ecdsaAlg = SignatureAlgorithm.CreateForVerification(ecdsa, HashAlgorithmName.SHA384)) {
                        _sut.SetSignatureAlgorithm(_dataRecord, ecdsaAlg, _encryptionKey);
                        
                        _dataRecord.SigType.Should().Be("ECDsa");
                        _dataRecord.SigHashAlgorithm.Should().Be(HashAlgorithmName.SHA384.Name);
                        _dataRecord.IsSigParameterEncrypted.Should().BeFalse();
                        _dataRecord.SigParameter.Should().Be(ecdsa.ExportParameters(false).ToXml());
                    }
                }
            }
        }

        public class ToSignatureAlgorithm : SignatureAlgorithmConverterTests {
            private readonly ClientDataRecord _dataRecord;
            private readonly SharedSecretEncryptionKey _encryptionKey;
            private readonly string _unencryptedKey;
            
            public ToSignatureAlgorithm() {
                _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
                _unencryptedKey = "s3cr3t";
                _dataRecord = new ClientDataRecord {
                    SigType = "HMAC",
                    SigParameter = new FakeStringProtector().Protect(_unencryptedKey),
                    SigHashAlgorithm = HashAlgorithmName.MD5.Name,
                    IsSigParameterEncrypted = true,
                    V = ClientDataRecord.GetV()
                };
            }

            [Fact]
            public void WhenDataRecordIsNull_ThrowsArgumentNullException() {
                Action act = () => _sut.ToSignatureAlgorithm(dataRecord: null, _encryptionKey);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenTypeIsNull_ThrowsNotSupportedException() {
                _dataRecord.SigType = null;
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey);
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void WhenTypeIsUnknown_ThrowsNotSupportedException() {
                _dataRecord.SigType = "custom_unsupported";
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey);
                act.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotThrow(string nullOrEmpty) {
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, (SharedSecretEncryptionKey)nullOrEmpty);
                act.Should().NotThrow();
            }

            [Theory]
            [InlineData("invalid_encryption_key")]
            [InlineData("the_big_secret")]
            public void GivenInvalidEncryptionKey_ThrowsSecurityException(string invalidKey) {
                _stringProtector.Throw(new FormatException("Invalid cypher."));
                
                Action act = () => _sut.ToSignatureAlgorithm(_dataRecord, (SharedSecretEncryptionKey)invalidKey);
                act.Should().Throw<SecurityException>();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyEncryptionKey_DoesNotDecryptParameter(string nullOrEmpty) {
                _dataRecord.SigParameter = _unencryptedKey;
                
                var actual = _sut.ToSignatureAlgorithm(_dataRecord, (SharedSecretEncryptionKey)nullOrEmpty);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }
            
            [Fact]
            public void WhenParameterIsNotEncrypted_DoesNotDecryptParameter() {
                _dataRecord.SigParameter = _unencryptedKey;
                _dataRecord.IsSigParameterEncrypted = false;
                
                var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey);

                actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                var actualKeyBytes = actual.As<HMACSignatureAlgorithm>().Key;
                var actualKey = Encoding.UTF8.GetString(actualKeyBytes);
                actualKey.Should().Be(_unencryptedKey);
            }
            
            [Fact]
            public void GivenRSADataRecord_ReturnsRSAAlgorithm() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicParameters = rsa.ExportParameters(includePrivateParameters: false);
                    _dataRecord.SigType = "RSA";
                    _dataRecord.SigParameter = publicParameters.ToXml();
                    _dataRecord.SigHashAlgorithm = HashAlgorithmName.MD5.Name;
                    _dataRecord.IsSigParameterEncrypted = false;
                    
                    using (var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey)) {
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
                    _dataRecord.SigType = "ECDsa";
                    _dataRecord.SigParameter = publicParameters.ToXml();
                    _dataRecord.SigHashAlgorithm = HashAlgorithmName.MD5.Name;
                    _dataRecord.IsSigParameterEncrypted = false;
                    
                    using (var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey)) {
                        var expected = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.MD5, publicParameters);
                        actual.Should().BeAssignableTo<ECDsaSignatureAlgorithm>();
                        actual.As<ECDsaSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                        actual.As<ECDsaSignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(publicParameters.ToXml());
                    }
                }
            }

            [Fact]
            public void GivenHMACDataRecord_ReturnsHMACDataRecord() {
                using (var actual = _sut.ToSignatureAlgorithm(_dataRecord, _encryptionKey)) {
                    var expected = new HMACSignatureAlgorithm(_unencryptedKey, HashAlgorithmName.MD5);
                    actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                    actual.As<HMACSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                }
            }
        }
    }
}