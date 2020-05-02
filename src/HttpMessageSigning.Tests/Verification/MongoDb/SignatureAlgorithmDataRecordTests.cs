using System;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class SignatureAlgorithmDataRecordTests {
        public class FromSignatureAlgorithm : SignatureAlgorithmDataRecordTests {
            [Fact]
            public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                Action act = () => SignatureAlgorithmDataRecord.FromSignatureAlgorithm(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenUnsupportedAlgorithmType_ThrowsNotSupportedException() {
                var unsupported = new CustomSignatureAlgorithm("CUSTOM");
                Action act = () => SignatureAlgorithmDataRecord.FromSignatureAlgorithm(unsupported);
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void GivenHMACAlgorithm_ReturnsExpectedDataRecord() {
                using (var hmac = SignatureAlgorithm.CreateForVerification("s3cr3t", HashAlgorithmName.SHA384)) {
                    var actual = SignatureAlgorithmDataRecord.FromSignatureAlgorithm(hmac);
                    var expected = new SignatureAlgorithmDataRecord {
                        Type = "HMAC",
                        Parameter = "s3cr3t",
                        HashAlgorithm = HashAlgorithmName.SHA384.Name
                    };
                    actual.Should().BeEquivalentTo(expected);
                }
            }

            [Fact]
            public void GivenRSAAlgorithm_ReturnsExpectedDataRecord() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    using (var rsaAlg = SignatureAlgorithm.CreateForVerification(rsa, HashAlgorithmName.SHA384)) {
                        var actual = SignatureAlgorithmDataRecord.FromSignatureAlgorithm(rsaAlg);
                        var expected = new SignatureAlgorithmDataRecord {
                            Type = "RSA",
                            Parameter = rsa.ExportParameters(false).ToXml(),
                            HashAlgorithm = HashAlgorithmName.SHA384.Name
                        };
                        actual.Should().BeEquivalentTo(expected);
                    }
                }
            }
        }


        public class ToSignatureAlgorithm : SignatureAlgorithmDataRecordTests {
            private readonly SignatureAlgorithmDataRecord _sut;

            public ToSignatureAlgorithm() {
                _sut = new SignatureAlgorithmDataRecord {
                    Type = "HMAC",
                    Parameter = "s3cr3t",
                    HashAlgorithm = HashAlgorithmName.MD5.Name
                };
            }

            [Fact]
            public void WhenTypeIsNull_ThrowsNotSupportedException() {
                _sut.Type = null;
                Action act = () => _sut.ToSignatureAlgorithm();
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void WhenTypeIsUnknown_ThrowsNotSupportedException() {
                _sut.Type = "custom_unsupported";
                Action act = () => _sut.ToSignatureAlgorithm();
                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public void GivenRSADataRecord_ReturnsRSAAlgorithm() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicParameters = rsa.ExportParameters(false);
                    var sut = new SignatureAlgorithmDataRecord {
                        Type = "RSA",
                        Parameter = publicParameters.ToXml(),
                        HashAlgorithm = HashAlgorithmName.MD5.Name
                    };

                    using (var actual = sut.ToSignatureAlgorithm()) {
                        var expected = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.MD5, publicParameters);
                        actual.Should().BeAssignableTo<RSASignatureAlgorithm>();
                        actual.As<RSASignatureAlgorithm>().Should().BeEquivalentTo(expected);
                        actual.As<RSASignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(publicParameters.ToXml());
                    }
                }
            }

            [Fact]
            public void GivenHMACDataRecord_ReturnsHMACDataRecord() {
                var sut = new SignatureAlgorithmDataRecord {
                    Type = "HMAC",
                    Parameter = "s3cr3t",
                    HashAlgorithm = HashAlgorithmName.MD5.Name
                };

                using (var actual = sut.ToSignatureAlgorithm()) {
                    var expected = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.MD5);
                    actual.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                    actual.As<HMACSignatureAlgorithm>().Should().BeEquivalentTo(expected);
                }
            }
        }
    }
}