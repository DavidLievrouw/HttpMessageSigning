using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbClientStoreTests : MongoIntegrationTest, IDisposable {
        private readonly MongoDbClientStore _sut;

        public MongoDbClientStoreTests(MongoSetup mongoSetup) : base(mongoSetup) {
            _sut = new MongoDbClientStore(new MongoDatabaseClientProvider(Database), "clients");
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
                var client = new Client("c1", "app one", hmac, new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().BeEquivalentTo(client);
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Secret.Should().Be("s3cr3t");
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }

            [Fact]
            public async Task CanRoundTripRSA() {
                using (var rsa = new RSACryptoServiceProvider()) {
                    var publicKeyParams = rsa.ExportParameters(false);
                    var rsaAlg = new RSASignatureAlgorithm(HashAlgorithmName.SHA384, publicKeyParams);
                    var client = new Client("c1", "app one", rsaAlg, new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                    await _sut.Register(client);

                    var actual = await _sut.Get(client.Id);

                    actual.Should().BeEquivalentTo(client);
                    actual.SignatureAlgorithm.Should().BeAssignableTo<RSASignatureAlgorithm>();
                    actual.SignatureAlgorithm.As<RSASignatureAlgorithm>().GetPublicKey().ToXml().Should().Be(rsa.ExportParameters(false).ToXml());
                    actual.SignatureAlgorithm.As<RSASignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
                }
            }

            [Fact]
            public async Task Upserts() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client1 = new Client("c1", "app one", hmac, new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client1);
                var client2 = new Client("c1", "app two", hmac, new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client2);

                var actual = await _sut.Get(client1.Id);

                actual.Should().BeEquivalentTo(client2);
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
            public void WhenClientIsNotFound_ThrowsSignatureVerificationException() {
                Func<Task> act = () => _sut.Get("IDontExist");
                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public async Task CanGetAndDeserializeExistingClient() {
                var hmac = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384);
                var client = new Client("c1", "app one", hmac, new Claim("company", "Dalion"), new Claim("scope", "HttpMessageSigning"));
                await _sut.Register(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().BeEquivalentTo(client);
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Secret.Should().Be("s3cr3t");
                actual.SignatureAlgorithm.As<HMACSignatureAlgorithm>().HashAlgorithm.Should().Be(HashAlgorithmName.SHA384);
            }
        }
    }
}