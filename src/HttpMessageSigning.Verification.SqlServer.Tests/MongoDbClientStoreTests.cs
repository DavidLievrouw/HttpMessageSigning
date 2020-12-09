using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class MongoDbClientStoreTests : MongoIntegrationTest, IDisposable {
        private readonly MongoDbClientStore _sut;
        private readonly string _collectionName;
        private readonly SharedSecretEncryptionKey _encryptionKey;

        public MongoDbClientStoreTests(MongoSetup mongoSetup) : base(mongoSetup) {
            _collectionName = "clients_" + Guid.NewGuid();
            _encryptionKey = new SharedSecretEncryptionKey("The_Big_Secret");
            _sut = new MongoDbClientStore(new MongoDatabaseClientProvider(Database), _collectionName, _encryptionKey);
        }

        public void Dispose() {
            _sut?.Dispose();
        }

        public class Construction : MongoDbClientStoreTests {
            public Construction(MongoSetup mongoSetup) : base(mongoSetup) { }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void AllowsForNullOrEmptyEncryptionKey(string nullOrEmpty) {
                Action act = () => new MongoDbClientStore(new MongoDatabaseClientProvider(Database), _collectionName, nullOrEmpty);
                act.Should().NotThrow();
            }
        }
        
        public class Register : MongoDbClientStoreTests {
            public Register(MongoSetup mongoSetup) : base(mongoSetup) { }

            [Fact]
            public void GivenNullClient_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData("_version")]
            public void DoesNotAcceptProhibitedIds(string prohibitedId) {
                var client = new Client(
                    (KeyId) prohibitedId, "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    ClientOptions.Default);

                Func<Task> act = () => _sut.Register(client);
                
                act.Should().Throw<ArgumentException>();
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
            public async Task EncryptsHMACSecretInDatabase() {
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
                
                var collection = Database.GetCollection<ClientDataRecord>(_collectionName);
                var findResult = await collection.FindAsync<ClientDataRecord>(new ExpressionFilterDefinition<ClientDataRecord>(r => r.Id == client.Id));
                var loaded = await findResult.SingleAsync();

                loaded.SignatureAlgorithm.Parameter.Should().NotBeNullOrEmpty();
                var unencryptedKey = Encoding.UTF8.GetString(hmac.Key);
                loaded.SignatureAlgorithm.Parameter.Should().NotBe(unencryptedKey);
                loaded.SignatureAlgorithm.IsParameterEncrypted.Should().BeTrue();
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
                
                var collection = Database.GetCollection<ClientDataRecord>(_collectionName);
                var findResult = await collection.FindAsync<ClientDataRecord>(new ExpressionFilterDefinition<ClientDataRecord>(r => r.Id == client.Id));
                var loaded = await findResult.SingleAsync();

                loaded.V.Should().NotBeNull();
                loaded.V.Should().Be(2); // Current version
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task WhenEncryptionKeyIsNullOrEmpty_DoesNotEncryptHMACSecretInDatabase(string nullOrEmpty) {
                using (var sut = new MongoDbClientStore(new MongoDatabaseClientProvider(Database), _collectionName, nullOrEmpty)) {
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

                    var collection = Database.GetCollection<ClientDataRecord>(_collectionName);
                    var findResult = await collection.FindAsync<ClientDataRecord>(new ExpressionFilterDefinition<ClientDataRecord>(r => r.Id == client.Id));
                    var loaded = await findResult.SingleAsync();

                    loaded.SignatureAlgorithm.Parameter.Should().NotBeNullOrEmpty();
                    var unencryptedKey = Encoding.UTF8.GetString(hmac.Key);
                    loaded.SignatureAlgorithm.Parameter.Should().Be(unencryptedKey);
                    loaded.SignatureAlgorithm.IsParameterEncrypted.Should().BeFalse();
                }
            }
        }

        public class Get : MongoDbClientStoreTests {
            public Get(MongoSetup mongoSetup) : base(mongoSetup) { }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData("_version")]
            public async Task WhenProhibitedIdIsSpecified_ReturnsNull(string prohibitedId) {
                var collection = Database.GetCollection<BsonDocument>(_collectionName);
                var legacyJson = @"{ 
    ""_id"" : """ + prohibitedId + @""", 
    ""Name"" : ""app one"", 
    ""SignatureAlgorithm"" : {
        ""Type"" : ""HMAC"", 
        ""Parameter"" : ""s3cr3t"", 
        ""HashAlgorithm"" : ""SHA384""
    }, 
    ""Claims"" : [
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""company"", 
            ""Value"" : ""Dalion"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }, 
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""scope"", 
            ""Value"" : ""HttpMessageSigning"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }
    ]
}";
                var legacyDocument = BsonSerializer.Deserialize<BsonDocument>(legacyJson);
                await collection.InsertOneAsync(legacyDocument);
                
                Client actual = null;
                Func<Task> act = async () => actual = await _sut.Get(prohibitedId);
                act.Should().NotThrow();
                actual.Should().BeNull();
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
            
            [Fact]
            public async Task CanDeserializeLegacyClientsWithoutNonceExpirationRequestTargetEscapingOrClockSkewOrVersion() {
                var collection = Database.GetCollection<BsonDocument>(_collectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c2"", 
    ""Name"" : ""app one"", 
    ""SignatureAlgorithm"" : {
        ""Type"" : ""HMAC"", 
        ""Parameter"" : ""s3cr3t"", 
        ""HashAlgorithm"" : ""SHA384""
    }, 
    ""Claims"" : [
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""company"", 
            ""Value"" : ""Dalion"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }, 
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""scope"", 
            ""Value"" : ""HttpMessageSigning"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }
    ]
}";
                var legacyDocument = BsonSerializer.Deserialize<BsonDocument>(legacyJson);
                await collection.InsertOneAsync(legacyDocument);
                
                var actual = await _sut.Get(new KeyId("c2"));

                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var expected = new Client(
                    "c2", 
                    "app one",
                    hmac, 
                    TimeSpan.FromMinutes(5), 
                    TimeSpan.FromMinutes(1), 
                    RequestTargetEscaping.RFC3986, 
                    new Claim("company", "Dalion"), 
                    new Claim("scope", "HttpMessageSigning"));
                actual.Should().BeEquivalentTo(expected, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes("s3cr3t"));
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
            
            [Fact]
            public async Task CanDeserializeLegacyClientsWithoutClockSkewOrVersion() {
                var collection = Database.GetCollection<BsonDocument>(_collectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c3"", 
    ""Name"" : ""app one"", 
    ""SignatureAlgorithm"" : {
        ""Type"" : ""HMAC"", 
        ""Parameter"" : ""s3cr3t"", 
        ""HashAlgorithm"" : ""SHA384""
    }, 
    ""NonceExpiration"" : 300.0,
    ""RequestTargetEscaping"" : ""RFC2396"",
    ""Claims"" : [
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""company"", 
            ""Value"" : ""Dalion"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }, 
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""scope"", 
            ""Value"" : ""HttpMessageSigning"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }
    ]
}";
                var legacyDocument = BsonSerializer.Deserialize<BsonDocument>(legacyJson);
                await collection.InsertOneAsync(legacyDocument);
                
                var actual = await _sut.Get(new KeyId("c3"));

                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var expected = new Client(
                    "c3", 
                    "app one", 
                    hmac, 
                    TimeSpan.FromMinutes(5), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC2396, 
                    new Claim("company", "Dalion"), 
                    new Claim("scope", "HttpMessageSigning"));
                actual.Should().BeEquivalentTo(expected, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes("s3cr3t"));
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
            
            [Fact]
            public async Task CanDeserializeLegacyClientsWithoutRequestTargetEscapingOrVersion() {
                var collection = Database.GetCollection<BsonDocument>(_collectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c4"", 
    ""Name"" : ""app one"", 
    ""SignatureAlgorithm"" : {
        ""Type"" : ""HMAC"", 
        ""Parameter"" : ""s3cr3t"", 
        ""HashAlgorithm"" : ""SHA384""
    }, 
    ""NonceExpiration"" : 300.0,
    ""ClockSkew"" : 240.0,
    ""Claims"" : [
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""company"", 
            ""Value"" : ""Dalion"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }, 
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""scope"", 
            ""Value"" : ""HttpMessageSigning"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }
    ]
}";
                var legacyDocument = BsonSerializer.Deserialize<BsonDocument>(legacyJson);
                await collection.InsertOneAsync(legacyDocument);
                
                var actual = await _sut.Get(new KeyId("c4"));

                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var expected = new Client(
                    "c4", 
                    "app one", 
                    hmac, 
                    TimeSpan.FromMinutes(5), 
                    TimeSpan.FromMinutes(4), 
                    RequestTargetEscaping.RFC3986, 
                    new Claim("company", "Dalion"), 
                    new Claim("scope", "HttpMessageSigning"));
                actual.Should().BeEquivalentTo(expected, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes("s3cr3t"));
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
            
            [Fact]
            public async Task CanDeserializeLegacyClientsWithoutVersion() {
                var collection = Database.GetCollection<BsonDocument>(_collectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c5"", 
    ""Name"" : ""app one"", 
    ""SignatureAlgorithm"" : {
        ""Type"" : ""HMAC"", 
        ""Parameter"" : ""s3cr3t"", 
        ""HashAlgorithm"" : ""SHA384""
    }, 
    ""NonceExpiration"" : 300.0,
    ""RequestTargetEscaping"" : ""RFC2396"",
    ""ClockSkew"" : 240.0,
    ""Claims"" : [
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""company"", 
            ""Value"" : ""Dalion"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }, 
        {
            ""Issuer"" : ""LOCAL AUTHORITY"", 
            ""OriginalIssuer"" : ""LOCAL AUTHORITY"", 
            ""Type"" : ""scope"", 
            ""Value"" : ""HttpMessageSigning"", 
            ""ValueType"" : ""http://www.w3.org/2001/XMLSchema#string""
        }
    ]
}";
                var legacyDocument = BsonSerializer.Deserialize<BsonDocument>(legacyJson);
                await collection.InsertOneAsync(legacyDocument);
                
                var actual = await _sut.Get(new KeyId("c5"));

                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var expected = new Client(
                    "c5", 
                    "app one", 
                    hmac, 
                    TimeSpan.FromMinutes(5), 
                    TimeSpan.FromMinutes(4), 
                    RequestTargetEscaping.RFC2396, 
                    new Claim("company", "Dalion"), 
                    new Claim("scope", "HttpMessageSigning"));
                actual.Should().BeEquivalentTo(expected, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes("s3cr3t"));
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
        }
    }
}