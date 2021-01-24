using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerClientStoreTests : SqlServerIntegrationTest {
        private readonly string _connectionString;
        private readonly SqlServerClientStore _sut;

        public SqlServerClientStoreTests(SqlServerFixture fixture)
            : base(fixture) {
            _sut = new SqlServerClientStore(new SqlServerClientStoreSettings {
                ConnectionString = fixture.SqlServerConfig.GetConnectionStringForTestDatabase()
            });
            _connectionString = fixture.SqlServerConfig.GetConnectionStringForTestDatabase();
        }

        public override void Dispose() {
            _sut?.Dispose();
            base.Dispose();
        }

        private async Task<ClientDataRecord> LoadFromDb(KeyId clientId) {
            using (var connection = new SqlConnection(_connectionString)) {
                string getSql = null;
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.GetClient.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = await streamReader.ReadToEndAsync();
                        getSql = template
                            .Replace("{ClientsTableName}", new SqlServerClientStoreSettings().ClientsTableName)
                            .Replace("{ClientClaimsTableName}", new SqlServerClientStoreSettings().ClientClaimsTableName);
                    }
                }

                var results = await connection.QueryAsync<ClientDataRecord, ClaimDataRecord, ClientDataRecord>(
                        getSql,
                        (client, claim) => {
                            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
                            client.Claims = client.Claims ?? new List<ClaimDataRecord>();
                            client.Claims.Add(claim);
                            return client;
                        },
                        new {ClientId = clientId.Value},
                        splitOn: nameof(ClaimDataRecord.ClientId))
                    .ConfigureAwait(continueOnCapturedContext: false);

                return results.FirstOrDefault();
            }
        }

        public class Register : SqlServerClientStoreTests {
            public Register(SqlServerFixture fixture)
                : base(fixture) { }

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
            public async Task CanRegisterMultipleClaimsWithSameTypes() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client(
                    "c1",
                    "app one",
                    hmac,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC2396,
                    new Claim("company", "Dalion"),
                    new Claim("scope", "HttpMessageSigning1"),
                    new Claim("scope", "HttpMessageSigning2"));
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
            public async Task EncryptsHMACSecretInDatabase() {
                using (var sut = new SqlServerClientStore(new SqlServerClientStoreSettings {
                    ConnectionString = _connectionString,
                    SharedSecretEncryptionKey = new SharedSecretEncryptionKey("The_Big_Secret")
                })) {
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

                    var loaded = await LoadFromDb(client.Id);

                    loaded.SigParameter.Should().NotBeNullOrEmpty();
                    var unencryptedKey = Encoding.UTF8.GetString(hmac.Key);
                    loaded.SigParameter.Should().NotBe(unencryptedKey);
                    loaded.IsSigParameterEncrypted.Should().BeTrue();
                }
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

                var loaded = await LoadFromDb(client.Id);

                loaded.V.Should().NotBeNull();
                loaded.V.Should().Be(1); // Current version
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task WhenEncryptionKeyIsNullOrEmpty_DoesNotEncryptHMACSecretInDatabase(string nullOrEmpty) {
                using (var sut = new SqlServerClientStore(new SqlServerClientStoreSettings {
                    ConnectionString = _connectionString,
                    SharedSecretEncryptionKey = nullOrEmpty
                })) {
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

                    var loaded = await LoadFromDb(client.Id);

                    loaded.SigParameter.Should().NotBeNullOrEmpty();
                    var unencryptedKey = Encoding.UTF8.GetString(hmac.Key);
                    loaded.SigParameter.Should().Be(unencryptedKey);
                    loaded.IsSigParameterEncrypted.Should().BeFalse();
                }
            }
        }

        public class Get : SqlServerClientStoreTests {
            public Get(SqlServerFixture fixture)
                : base(fixture) { }

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