using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations.V0002 {
    public class AddEncryptionSupportToClientsMigrationStepTests : MongoIntegrationTest {
        private readonly MongoDbClientStoreSettings _settings;
        private readonly AddEncryptionSupportToClientsMigrationStep _sut;

        public AddEncryptionSupportToClientsMigrationStepTests(MongoSetup mongoSetup)
            : base(mongoSetup) {
            _settings = new MongoDbClientStoreSettings {
                CollectionName = "clients",
                SharedSecretEncryptionKey = new SharedSecretEncryptionKey("The_Big_Secret"),
                ConnectionString = "dummy",
                ClientCacheEntryExpiration = TimeSpan.Zero
            };
            _sut = new AddEncryptionSupportToClientsMigrationStep(new MongoDatabaseClientProvider(Database), _settings);
        }

        public class Run : AddEncryptionSupportToClientsMigrationStepTests {
            private readonly IMongoCollection<ClientDataRecordV2> _mongoCollection;

            public Run(MongoSetup mongoSetup)
                : base(mongoSetup) {
                _mongoCollection = Database.GetCollection<ClientDataRecordV2>(_settings.CollectionName);
                _mongoCollection.DeleteMany(FilterDefinition<ClientDataRecordV2>.Empty);
            }

            [Fact]
            public async Task WhenThereAreNoClients_DoesNothing() {
                await _sut.Run();

                var findResult = await _mongoCollection.FindAsync<ClientDataRecordV2>(FilterDefinition<ClientDataRecordV2>.Empty);
                var clients = await findResult.ToListAsync();
                clients.Should().BeEmpty();
            }

            [Fact]
            public async Task DoesNotMigrateUpToDateClients() {
                var upToDateClient = new ClientDataRecordV2 {
                    Id = "c1",
                    Name = "Up-to-date client",
                    ClockSkew = 60,
                    NonceLifetime = 30,
#pragma warning disable 618
                    NonceExpiration = null,
#pragma warning restore 618
                    SignatureAlgorithm = new SignatureAlgorithmDataRecordV2 {
                        Type = "HMAC",
                        Parameter = "s3cr37",
                        HashAlgorithm = "SHA384",
                        IsParameterEncrypted = false
                    },
                    V = 2,
                    RequestTargetEscaping = "RFC3986",
                    Claims = new[] {
                        ClaimDataRecordV2.FromClaim(new Claim("company", "Dalion"))
                    }
                };

                await _mongoCollection.InsertOneAsync(upToDateClient);

                await _sut.Run();

                var findResult = await _mongoCollection.FindAsync<ClientDataRecordV2>(new ExpressionFilterDefinition<ClientDataRecordV2>(r => r.Id == upToDateClient.Id));
                var loaded = await findResult.SingleAsync();

                loaded.Should().BeEquivalentTo(upToDateClient);
            }

            [Fact]
            public async Task MigratesApplicableClientsAndEncryptsParameters() {
                var upToDateClient = new ClientDataRecordV2 {
                    Id = "c2",
                    Name = "Up-to-date client",
                    ClockSkew = 60,
                    NonceLifetime = 30,
#pragma warning disable 618
                    NonceExpiration = null,
#pragma warning restore 618
                    SignatureAlgorithm = new SignatureAlgorithmDataRecordV2 {
                        Type = "HMAC",
                        Parameter = "s3cr37",
                        HashAlgorithm = "SHA384",
                        IsParameterEncrypted = false
                    },
                    V = 2,
                    RequestTargetEscaping = "RFC3986",
                    Claims = new[] {
                        ClaimDataRecordV2.FromClaim(new Claim("company", "Dalion"))
                    }
                };
                await _mongoCollection.InsertOneAsync(upToDateClient);

                var bsonCollection = Database.GetCollection<BsonDocument>(_settings.CollectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c3"", 
    ""Name"" : ""Legacy client"", 
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
                await bsonCollection.InsertOneAsync(legacyDocument);

                await _sut.Run();

                var findUpToDateResult = await _mongoCollection.FindAsync<ClientDataRecordV2>(new ExpressionFilterDefinition<ClientDataRecordV2>(r => r.Id == upToDateClient.Id));
                var loadedUpToDate = await findUpToDateResult.SingleAsync();

                loadedUpToDate.Should().BeEquivalentTo(upToDateClient);

                var findLegacyResult = await _mongoCollection.FindAsync<ClientDataRecordV2>(new ExpressionFilterDefinition<ClientDataRecordV2>(r => r.Id == "c3"));
                var loadedLegacy = await findLegacyResult.SingleAsync();

                var expectedMigratedClient = new ClientDataRecordV2 {
                    Id = "c3",
                    Name = "Legacy client",
                    ClockSkew = 60.0,
                    NonceLifetime = 300.0,
#pragma warning disable 618
                    NonceExpiration = null,
#pragma warning restore 618
                    SignatureAlgorithm = new SignatureAlgorithmDataRecordV2 {
                        Type = "HMAC",
                        Parameter = "s3cr3t",
                        HashAlgorithm = "SHA384",
                        IsParameterEncrypted = true
                    },
                    V = 2,
                    RequestTargetEscaping = "RFC3986",
                    Claims = new[] {
                        ClaimDataRecordV2.FromClaim(new Claim("company", "Dalion")),
                        ClaimDataRecordV2.FromClaim(new Claim("scope", "HttpMessageSigning"))
                    }
                };
                loadedLegacy.Should().BeEquivalentTo(expectedMigratedClient, options => options.Excluding(_ => _.SignatureAlgorithm.Parameter));
                loadedLegacy.SignatureAlgorithm.Parameter.Should().NotBeNull().And.NotBe("s3cr3t");
            }

            [Fact]
            public async Task WhenEncryptionIsDisabled_DoesNotEncryptParameters() {
                _settings.SharedSecretEncryptionKey = SharedSecretEncryptionKey.Empty;
                
                var bsonCollection = Database.GetCollection<BsonDocument>(_settings.CollectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c4"", 
    ""Name"" : ""Legacy client"", 
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
                await bsonCollection.InsertOneAsync(legacyDocument);

                await _sut.Run();

                var findLegacyResult = await _mongoCollection.FindAsync<ClientDataRecordV2>(new ExpressionFilterDefinition<ClientDataRecordV2>(r => r.Id == "c4"));
                var loadedLegacy = await findLegacyResult.SingleAsync();

                var expectedMigratedClient = new ClientDataRecordV2 {
                    Id = "c4",
                    Name = "Legacy client",
                    ClockSkew = 60.0,
                    NonceLifetime = 300.0,
#pragma warning disable 618
                    NonceExpiration = null,
#pragma warning restore 618
                    SignatureAlgorithm = new SignatureAlgorithmDataRecordV2 {
                        Type = "HMAC",
                        Parameter = "s3cr3t",
                        HashAlgorithm = "SHA384",
                        IsParameterEncrypted = false
                    },
                    V = 2,
                    RequestTargetEscaping = "RFC3986",
                    Claims = new[] {
                        ClaimDataRecordV2.FromClaim(new Claim("company", "Dalion")),
                        ClaimDataRecordV2.FromClaim(new Claim("scope", "HttpMessageSigning"))
                    }
                };
                loadedLegacy.Should().BeEquivalentTo(expectedMigratedClient);
            }

            [Fact]
            public async Task DoesNotEncryptParametersOfNonHMACClients() {
                var bsonCollection = Database.GetCollection<BsonDocument>(_settings.CollectionName);
                var legacyJson = @"{ 
    ""_id"" : ""c5"", 
    ""Name"" : ""Legacy client"", 
    ""SignatureAlgorithm"" : {
        ""Type"" : ""RSA"", 
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
                await bsonCollection.InsertOneAsync(legacyDocument);

                await _sut.Run();

                var findLegacyResult = await _mongoCollection.FindAsync<ClientDataRecordV2>(new ExpressionFilterDefinition<ClientDataRecordV2>(r => r.Id == "c5"));
                var loadedLegacy = await findLegacyResult.SingleAsync();

                var expectedMigratedClient = new ClientDataRecordV2 {
                    Id = "c5",
                    Name = "Legacy client",
                    ClockSkew = 60.0,
                    NonceLifetime = 300.0,
#pragma warning disable 618
                    NonceExpiration = null,
#pragma warning restore 618
                    SignatureAlgorithm = new SignatureAlgorithmDataRecordV2 {
                        Type = "RSA",
                        Parameter = "s3cr3t",
                        HashAlgorithm = "SHA384",
                        IsParameterEncrypted = false
                    },
                    V = 2,
                    RequestTargetEscaping = "RFC3986",
                    Claims = new[] {
                        ClaimDataRecordV2.FromClaim(new Claim("company", "Dalion")),
                        ClaimDataRecordV2.FromClaim(new Claim("scope", "HttpMessageSigning"))
                    }
                };
                loadedLegacy.Should().BeEquivalentTo(expectedMigratedClient);
            }
        }

        public class Version : AddEncryptionSupportToClientsMigrationStepTests {
            public Version(MongoSetup mongoSetup)
                : base(mongoSetup) { }

            [Fact]
            public void ReturnsExpectedVersion() {
                var actual = _sut.Version;
                actual.Should().Be(2);
            }
        }
    }
}