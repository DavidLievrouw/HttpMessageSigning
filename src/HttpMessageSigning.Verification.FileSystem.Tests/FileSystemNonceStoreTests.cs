using System;
using System.IO;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Utils;
using Dalion.HttpMessageSigning.Verification.FileSystem.Serialization;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class FileSystemNonceStoreTests : IDisposable {
        private readonly IFileManager<NonceDataRecord> _fileManager;
        private readonly ISystemClock _systemClock;
        private readonly FileSystemNonceStore _sut;
        private readonly DateTimeOffset _now;

        public FileSystemNonceStoreTests() {
            FakeFactory.Create(out _systemClock);
            _now = new DateTimeOffset(
                DateTimeOffset.UtcNow.Year,
                DateTimeOffset.UtcNow.Month,
                DateTimeOffset.UtcNow.Day,
                DateTimeOffset.UtcNow.Hour,
                DateTimeOffset.UtcNow.Minute,
                DateTimeOffset.UtcNow.Second,
                DateTimeOffset.UtcNow.Millisecond,
                TimeSpan.Zero);
            A.CallTo(() => _systemClock.UtcNow)
                .Returns(_now);

            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            _fileManager = new LockingFileManager<NonceDataRecord>(
                new NoncesFileManager(
                    new FileReader(),
                    new FileWriter(),
                    tempFilePath,
                    new NonceDataRecordSerializer()),
                new SemaphoreFactory());

            _sut = new FileSystemNonceStore(_fileManager, _systemClock);
        }

        public void Dispose() {
            _sut?.Dispose();
        }

        public class Register : FileSystemNonceStoreTests {
            [Fact]
            public void GivenNullNonce_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
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
            public async Task DeletesExpiredNonces() {
                var dataRecords = new[] {
                    new NonceDataRecord {ClientId = "c1", Value = "abc", Expiration = _now.AddMinutes(1)},
                    new NonceDataRecord {ClientId = "c1", Value = "abd", Expiration = _now.AddMinutes(0)},
                    new NonceDataRecord {ClientId = "c1", Value = "abe", Expiration = _now.AddMinutes(-1)},
                    new NonceDataRecord {ClientId = "c2", Value = "abc", Expiration = _now.AddMinutes(-1)},
                };
                await _fileManager.Write(dataRecords);

                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddMinutes(1));
                await _sut.Register(nonce);

                var nonceDataRecords = await _fileManager.Read();

                var expectedNonceDataRecords = new[] {
                    new NonceDataRecord {ClientId = "c1", Value = "abc", Expiration = _now.AddMinutes(1)},
                    new NonceDataRecord {ClientId = "c1", Value = "abc123", Expiration = _now.AddMinutes(1)}
                };
                nonceDataRecords.Should().BeEquivalentTo<NonceDataRecord>(expectedNonceDataRecords);
            }
        }

        public class Get : FileSystemNonceStoreTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(nullOrEmpty, "abc123");
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyNonceValue_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(new KeyId("c1"), nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public async Task WhenNonceIsNotFound_ReturnsNull() {
                var actual = await _sut.Get("IDontExist", "abc123");
                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenMultipleNoncesAreRegistered_ReturnsTheLatestOne() {
                var clientId = new KeyId("c1");
                var nonceValue = "abc123";

                var nonce1 = new Nonce(clientId, nonceValue, _now.AddMinutes(-1));
                await _sut.Register(nonce1);
                var nonce2 = new Nonce(clientId, nonceValue, _now.AddMinutes(1));
                await _sut.Register(nonce2);
                var nonce3 = new Nonce(clientId, nonceValue, _now.AddMinutes(2));
                await _sut.Register(nonce3);
                var nonce4 = new Nonce(clientId, nonceValue, _now.AddMinutes(1.5));
                await _sut.Register(nonce4);

                var actual = await _sut.Get(clientId, nonceValue);

                actual.Should().BeEquivalentTo(nonce4);
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