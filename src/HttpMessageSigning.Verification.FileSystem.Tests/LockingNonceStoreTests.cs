using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Nito.AsyncEx;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class LockingNonceStoreTests : IDisposable {
        private readonly INonceStore _decorated;
        private readonly AsyncReaderWriterLock _lock;
        private readonly ILockFactory _lockFactory;
        private readonly LockingNonceStore _sut;

        public LockingNonceStoreTests() {
            FakeFactory.Create(out _decorated, out _lockFactory);
            _lock = new AsyncReaderWriterLock();
            A.CallTo(() => _lockFactory.CreateLock())
                .Returns(_lock);
            _sut = new LockingNonceStore(_decorated, _lockFactory);
        }

        public void Dispose() {
            _sut?.Dispose();
        }

        public class DisposableSupport : LockingNonceStoreTests {
            [Fact]
            public void DisposesDecoratedService() {
                _sut.Dispose();

                A.CallTo(() => _decorated.Dispose())
                    .MustHaveHappened();
            }
        }

        public class Register : LockingNonceStoreTests {
            [Fact]
            public async Task CallsDecoratedService() {
                var nonce = new Nonce((KeyId)"c1", "abc123", DateTimeOffset.UtcNow.AddMinutes(1));

                await _sut.Register(nonce);

                A.CallTo(() => _decorated.Register(nonce))
                    .MustHaveHappened();
            }
        }

        public class Get : LockingNonceStoreTests {
            [Fact]
            public async Task ReturnsResultFromDecoratedService() {
                var nonce = new Nonce((KeyId)"c1", "abc123", DateTimeOffset.UtcNow.AddMinutes(1));

                A.CallTo(() => _decorated.Get(nonce.ClientId, nonce.Value))
                    .Returns(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().Be(nonce);
            }
        }
    }
}