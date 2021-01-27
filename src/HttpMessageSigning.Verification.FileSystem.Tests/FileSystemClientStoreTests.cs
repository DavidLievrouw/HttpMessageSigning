using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.FileSystem.Serialization;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class FileSystemClientStoreTests : IDisposable {
        private readonly FileSystemClientStore _sut;
        private readonly IFileManager<ClientDataRecord> _fileManager;
        private readonly ISignatureAlgorithmDataRecordConverter _signatureAlgorithmDataRecordConverter;

        public FileSystemClientStoreTests() {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            _fileManager = new ClientsFileManager(
                new FileReader(),
                new FileWriter(),
                tempFilePath,
                new ClientDataRecordSerializer());
            var encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
            _signatureAlgorithmDataRecordConverter = new SignatureAlgorithmDataRecordConverter(new FakeStringProtectorFactory());
            _sut = new FileSystemClientStore(_fileManager, _signatureAlgorithmDataRecordConverter, encryptionKey);
        }

        public void Dispose() {
            (_fileManager as IDisposable)?.Dispose();
            _sut?.Dispose();
        }

        public class Construction : FileSystemClientStoreTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void AllowsForNullOrEmptyEncryptionKey(string nullOrEmpty) {
                Action act = () => new FileSystemClientStore(_fileManager, _signatureAlgorithmDataRecordConverter, nullOrEmpty);
                act.Should().NotThrow();
            }
        }

        public class Register : FileSystemClientStoreTests {
            [Fact]
            public void GivenNullClient_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task CanRoundTripHMAC() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client(
                    "c1",
                    "app one",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC2396,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes("s3cr3t"));
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }

            [Fact]
            public async Task CanRoundTripRSA() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicKeyParams = rsa.ExportParameters(false);
                    var rsaAlg = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA384, publicKeyParams);
                    var client = new Client(
                        "c1",
                        "app one",
                        rsaAlg,
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC2396,
                        new Claim("company", "Dalion"),
                        new Claim("scope", "HttpMessageSigning"));
                    await _sut.Register(client);

                    var actual = await _sut.Get(client.Id);

                    actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                    actual.SignatureAlgorithm.Should().BeAssignableTo<RSASignatureAlgorithm>();
                    actual.SignatureAlgorithm.As<RSASignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(rsa.ExportParameters(false).ToXml());
                    actual.SignatureAlgorithm.As<RSASignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
                }
            }

            [Fact]
            public async Task CanRoundTripECDsa() {
                using (var ecdsa = ECDsa.Create()) {
                    var publicKeyParams = ecdsa.ExportParameters(false);
                    var rsaAlg = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA384, publicKeyParams);
                    var client = new Client(
                        "c1",
                        "app one",
                        rsaAlg,
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC2396,
                        new Claim("company", "Dalion"),
                        new Claim("scope", "HttpMessageSigning"));
                    await _sut.Register(client);

                    var actual = await _sut.Get(client.Id);

                    actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                    actual.SignatureAlgorithm.Should().BeAssignableTo<ECDsaSignatureAlgorithm>();
                    actual.SignatureAlgorithm.As<ECDsaSignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(ecdsa.ExportParameters(false).ToXml());
                    actual.SignatureAlgorithm.As<ECDsaSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
                }
            }

            [Fact]
            public async Task Upserts() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client1 = new Client(
                    "c1",
                    "app one",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC3986,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client1);
                var client2 = new Client(
                    "c1",
                    "app two",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(3),
                    RequestTargetEscaping.OriginalString,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client2);

                var actual = await _sut.Get(client1.Id);

                actual.Should().BeEquivalentTo(client2, options => options.ComparingByMembers<Client>());
            }

            [Fact]
            public async Task EncryptsHMACSecretInFile() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client(
                    "c1",
                    "app one",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC2396,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var dataRecords = await _fileManager.Read();
                var loaded = dataRecords.Single(r => r.Id == client.Id);

                loaded.SigAlg.Param.Should().NotBeNullOrEmpty();
                var unencryptedKey = Encoding.UTF8.GetString(hmac.Key);
                var encryptedKey = new FakeStringProtector().Protect(unencryptedKey);
                loaded.SigAlg.Param.Should().Be($"<Secret>{encryptedKey}</Secret>");
                loaded.SigAlg.Encrypted.Should().BeTrue();
            }

            [Fact]
            public async Task MarksRecordsWithCorrectVersion() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client(
                    "c1",
                    "app one",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC2396,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var dataRecords = await _fileManager.Read();
                var loaded = dataRecords.Single(r => r.Id == client.Id);

                loaded.V.Should().NotBeNull();
                loaded.V.Should().Be(1); // Current version
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task WhenEncryptionKeyIsNullOrEmpty_DoesNotEncryptHMACSecretInFile(string nullOrEmpty) {
                using (var sut = new FileSystemClientStore(_fileManager, _signatureAlgorithmDataRecordConverter, nullOrEmpty)) {
                    var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                    var client = new Client(
                        "c1",
                        "app one",
                        hmac,
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromMinutes(2),
                        RequestTargetEscaping.RFC2396,
                        new Claim("company", "Dalion"),
                        new Claim("scope", "HttpMessageSigning"));
                    await sut.Register(client);

                    var dataRecords = await _fileManager.Read();
                    var loaded = dataRecords.Single(r => r.Id == client.Id);

                    loaded.SigAlg.Param.Should().NotBeNullOrEmpty();
                    var unencryptedKey = Encoding.UTF8.GetString(hmac.Key);
                    loaded.SigAlg.Param.Should().Be($"<Secret>{unencryptedKey}</Secret>");
                    loaded.SigAlg.Encrypted.Should().BeFalse();
                }
            }
        }

        public class Get : FileSystemClientStoreTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void WhenClientIsNotFound_ReturnsNull() {
                Client actual = null;
                Func<Task> act = async () => actual = await _sut.Get("IDontExist");
                act.Should().NotThrow();
                actual.Should().BeNull();
            }

            [Fact]
            public async Task CanGetAndDeserializeExistingClient() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client(
                    "c1",
                    "app one",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.Unescaped,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes("s3cr3t"));
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
        }
    }
}