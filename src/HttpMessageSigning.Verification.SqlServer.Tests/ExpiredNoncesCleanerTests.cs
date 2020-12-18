using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using Dapper;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class ExpiredNoncesCleanerTests : SqlServerIntegrationTest {
        private readonly ImmediateBackgroundTaskStarter _backgroundTaskStarter;
        private readonly TimeSpan _cleanUpInterval;
        private readonly string _connectionString;
        private readonly ExpiredNoncesCleaner _sut;
        private readonly ISystemClock _systemClock;
        private readonly string _tableName;

        public ExpiredNoncesCleanerTests(SqlServerFixture fixture)
            : base(fixture) {
            _systemClock = A.Fake<ISystemClock>();
            _backgroundTaskStarter = new ImmediateBackgroundTaskStarter();
            _cleanUpInterval = TimeSpan.FromMinutes(1);
            _tableName = "Nonces";
            _connectionString = fixture.SqlServerConfig.GetConnectionStringForTestDatabase();
            _sut = new ExpiredNoncesCleaner(_connectionString, _tableName, _backgroundTaskStarter, _systemClock, _cleanUpInterval);
        }

        public override void Dispose() {
            _sut?.Dispose();
            base.Dispose();
        }

        public class CleanUpNonces : ExpiredNoncesCleanerTests {
            private readonly DateTimeOffset _now;

            public CleanUpNonces(SqlServerFixture fixture)
                : base(fixture) {
                _now = new DateTimeOffset(2020, 12, 18, 13, 57, 21, 123, TimeSpan.Zero);
                A.CallTo(() => _systemClock.UtcNow)
                    .Returns(_now);
            }

            [Fact]
            public async Task StartsBackgroundTask() {
                _backgroundTaskStarter.InvocationCount.Should().Be(0);
                
                await _sut.CleanUpNonces();

                _backgroundTaskStarter.InvocationCount.Should().Be(1);
            }

            [Fact]
            public async Task WhenTaskIsStartedWithinSpecifiedInterval_DoesNothing() {
                _backgroundTaskStarter.InvocationCount.Should().Be(0);
                
                await _sut.CleanUpNonces();
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                await _sut.CleanUpNonces();
                
                _backgroundTaskStarter.InvocationCount.Should().Be(1);
            }

            [Fact]
            public async Task DeletesExpiredNonces() {
                var noncesToInsert = new[] {
                    new Nonce(new KeyId("c1"), "abc001", _now.AddSeconds(_cleanUpInterval.TotalSeconds / 2 * -1)),
                    new Nonce(new KeyId("c1"), "abc002", _now),
                    new Nonce(new KeyId("c1"), "abc003", _now.AddSeconds(30)),
                    new Nonce(new KeyId("c1"), "abc004", _now.AddMinutes(30)),
                    new Nonce(new KeyId("c1"), "abc005", _now.AddSeconds(_cleanUpInterval.TotalSeconds * -1)),
                    new Nonce(new KeyId("c1"), "abc006", _now.AddSeconds(_cleanUpInterval.TotalSeconds * -2))
                };
                
                var noopExpiredNonceCleaner = A.Fake<IExpiredNoncesCleaner>();
                using (var nonceStore = new SqlServerNonceStore(_connectionString, _tableName, noopExpiredNonceCleaner)) {
                    foreach (var nonce in noncesToInsert) {
                        await nonceStore.Register(nonce);
                    }
                }

                var allNoncesBefore = await GetAllNonces();
                allNoncesBefore.Should().BeEquivalentTo<Nonce>(noncesToInsert);

                await _sut.CleanUpNonces();

                var allNoncesAfter = await GetAllNonces();
                var expectedNonces = new[] {
                    new Nonce(new KeyId("c1"), "abc003", _now.AddSeconds(30)),
                    new Nonce(new KeyId("c1"), "abc004", _now.AddMinutes(30)),
                };
                allNoncesAfter.Should().BeEquivalentTo<Nonce>(expectedNonces);
            }

            private async Task<IEnumerable<Nonce>> GetAllNonces() {
                var sql = @"SELECT [ClientId], [Value], [Expiration] FROM " + _tableName;
                
                using (var connection = new SqlConnection(_connectionString)) {
                    var nonces = await connection.QueryAsync<NonceDataRecord>(sql);
                    return nonces.Select(nonce => new Nonce(new KeyId(nonce.ClientId), nonce.Value, nonce.Expiration));
                }
            }
        }
    }
}