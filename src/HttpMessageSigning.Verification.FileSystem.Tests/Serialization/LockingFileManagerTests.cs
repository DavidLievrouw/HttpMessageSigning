using System;
using System.Threading;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class LockingFileManagerTests : IDisposable {
        private readonly IFileManager<ClientDataRecord> _decorated;
        private readonly SemaphoreSlim _semaphore;
        private readonly ISemaphoreFactory _semaphoreFactory;
        private readonly LockingFileManager<ClientDataRecord> _sut;

        public LockingFileManagerTests() {
            FakeFactory.Create(out _decorated, out _semaphoreFactory);
            _semaphore = new SemaphoreSlim(1, 1);
            A.CallTo(() => _semaphoreFactory.CreateLock())
                .Returns(_semaphore);
            _sut = new LockingFileManager<ClientDataRecord>(_decorated, _semaphoreFactory);
        }

        public void Dispose() {
            _semaphore?.Dispose();
            _sut?.Dispose();
        }

        public class Write : LockingFileManagerTests {
            [Fact]
            public async Task CallsDecoratedService() {
                var clients = new[] {
                    new ClientDataRecord {Id = "client001"},
                    new ClientDataRecord {Id = "client002"}
                };

                await _sut.Write(clients);

                A.CallTo(() => _decorated.Write(clients))
                    .MustHaveHappened();
            }
        }

        public class Read : LockingFileManagerTests {
            [Fact]
            public async Task ReturnsResultFromDecoratedService() {
                var clients = new[] {
                    new ClientDataRecord {Id = "client001"},
                    new ClientDataRecord {Id = "client002"}
                };
                A.CallTo(() => _decorated.Read())
                    .Returns(clients);

                var actual = await _sut.Read();

                actual.Should().Equal(clients);
            }
        }
    }
}