using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using Dapper;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    [DisableTransactionScope]
    public class SqlServerNonceStoreTests : SqlServerIntegrationTest {
        private readonly IExpiredNoncesCleaner _expiredNoncesCleaner;
        private readonly DateTimeOffset _now;
        private readonly SqlServerNonceStore _sut;
        private readonly SqlServerNonceStoreSettings _settings;

        public SqlServerNonceStoreTests(SqlServerFixture fixture)
            : base(fixture) {
            FakeFactory.Create(out _expiredNoncesCleaner);
            _settings = new SqlServerNonceStoreSettings {
                ExpiredNoncesCleanUpInterval = TimeSpan.FromMinutes(1),
                ConnectionString = fixture.SqlServerConfig.GetConnectionStringForTestDatabase()
            };
            _sut = new SqlServerNonceStore(_settings, _expiredNoncesCleaner);

            _now = new DateTimeOffset(
                DateTimeOffset.UtcNow.Year,
                DateTimeOffset.UtcNow.Month,
                DateTimeOffset.UtcNow.Day,
                DateTimeOffset.UtcNow.Hour,
                DateTimeOffset.UtcNow.Minute,
                DateTimeOffset.UtcNow.Second,
                DateTimeOffset.UtcNow.Millisecond,
                TimeSpan.Zero);
        }

        public override void Dispose() {
            _sut?.Dispose();
            base.Dispose();
        }

        public class Register : SqlServerNonceStoreTests {
            public Register(SqlServerFixture fixture)
                : base(fixture) {
            }

            [Fact]
            public async Task GivenNullNonce_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }

            [Fact]
            public async Task CanRoundTrip() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddSeconds(30));

                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
                actual.Expiration.Offset.Should().Be(TimeSpan.Zero);
            }

            [Fact]
            public async Task Upserts() {
                var nonce1 = new Nonce(new KeyId("c1"), "abc123", _now.AddMinutes(1));
                await _sut.Register(nonce1);

                var nonce2 = new Nonce(nonce1.ClientId, nonce1.Value, _now.AddMinutes(2));
                await _sut.Register(nonce2);

                var actual = await _sut.Get(nonce1.ClientId, nonce1.Value);

                actual.Should().BeEquivalentTo(nonce2);
            }

            [Fact]
            public async Task WhenInsertingADuplicateNonce_UpdatesExpiration() {
                var clientId = new KeyId("c1");
                var nonceValue = "abc123";

                var nonce1 = new Nonce(clientId, nonceValue, _now.AddMinutes(-1));
                await _sut.Register(nonce1);

                var nonce2 = new Nonce(clientId, nonceValue, _now.AddMinutes(1));
                await _sut.Register(nonce2);

                var allNonces = await GetAllNonces();
                var expected = new[] {nonce2};
                allNonces.Should().BeEquivalentTo<Nonce>(expected);
            }

            [Fact]
            public async Task CleansUpExpiredNonces() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddSeconds(30));

                await _sut.Register(nonce);

                A.CallTo(() => _expiredNoncesCleaner.CleanUpNonces())
                    .MustHaveHappened();
            }
            
            private async Task<IEnumerable<Nonce>> GetAllNonces() {
                var sql = @"SELECT [ClientId], [Value], [Expiration] FROM " + _settings.NonceTableName;
                
                using (var connection = new SqlConnection(_settings.ConnectionString)) {
                    var nonces = await connection.QueryAsync<NonceDataRecord>(sql);
                    return nonces.Select(nonce => new Nonce(new KeyId(nonce.ClientId), nonce.Value, nonce.Expiration));
                }
            }
        }

        public class Get : SqlServerNonceStoreTests {
            public Get(SqlServerFixture fixture)
                : base(fixture) { }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get((KeyId)nullOrEmpty, "abc123");
                await act.Should().ThrowAsync<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task GivenNullOrEmptyNonceValue_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(new KeyId("c1"), nullOrEmpty);
                await act.Should().ThrowAsync<ArgumentException>();
            }

            [Fact]
            public async Task WhenNonceIsNotFound_ReturnsNull() {
                var actual = await _sut.Get((KeyId)"IDontExist", "abc123");
                actual.Should().BeNull();
            }

            [Fact]
            public async Task CanGetAndDeserializeExistingNonce() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddMinutes(1));
                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
            }
        }
    }
}