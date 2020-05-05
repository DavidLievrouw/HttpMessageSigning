using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbClientStoreTests : MongoIntegrationTest, IDisposable {
        private readonly MongoDbClientStore _sut;
        private readonly string _collectionName;

        public MongoDbClientStoreTests(MongoSetup mongoSetup) : base(mongoSetup) {
            _collectionName = "clients";
            _sut = new MongoDbClientStore(new MongoDatabaseClientProvider(Database), _collectionName);
        }

        public void Dispose() {
            _sut?.Dispose();
        }

        public class Register : MongoDbClientStoreTests {
            public Register(MongoSetup mongoSetup) : base(mongoSetup) { }

            [Fact]
            public void GivenNullClient_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task CanRoundTripHMAC() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client("c1", "app one", hmac, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Secret.Should().Be("s3cr3t");
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }

            [Fact]
            public async Task CanRoundTripRSA() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicKeyParams = rsa.ExportParameters(false);
                    var rsaAlg = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA384, publicKeyParams);
                    var client = new Client("c1", "app one", rsaAlg, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                    await _sut.Register(client);

                    var actual = await _sut.Get(client.Id);

                    actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                    actual.SignatureAlgorithm.Should().BeAssignableTo<RSASignatureAlgorithm>();
                    actual.SignatureAlgorithm.As<RSASignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(rsa.ExportParameters(false).ToXml());
                    actual.SignatureAlgorithm.As<RSASignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
                }
            }

            [Fact]
            public async Task Upserts() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client1 = new Client("c1", "app one", hmac, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client1);
                var client2 = new Client("c1", "app two", hmac, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(3), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client2);

                var actual = await _sut.Get(client1.Id);

                actual.Should().BeEquivalentTo(client2, options => options.ComparingByMembers<Client>());
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

            [Fact]
            public void WhenClientIsNotFound_ThrowsInvalidClientException() {
                Func<Task> act = () => _sut.Get("IDontExist");
                act.Should().Throw<InvalidClientException>();
            }

            [Fact]
            public async Task CanGetAndDeserializeExistingClient() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client("c1", "app one", hmac, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().BeEquivalentTo(client, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Secret.Should().Be("s3cr3t");
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
            
            [Fact]
            public async Task CanDeserializeLegacyClientsWithoutNonceExpirationOrClockSkew() {
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
                var expected = new Client("c2", "app one", hmac, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                actual.Should().BeEquivalentTo(expected, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Secret.Should().Be("s3cr3t");
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
            
            [Fact]
            public async Task CanDeserializeLegacyClientsWithoutClockSkew() {
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
                var expected = new Client("c3", "app one", hmac, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1), new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                actual.Should().BeEquivalentTo(expected, options => options.ComparingByMembers<Client>());
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Secret.Should().Be("s3cr3t");
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
        }
    }
}