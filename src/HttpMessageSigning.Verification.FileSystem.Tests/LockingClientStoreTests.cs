using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Nito.AsyncEx;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class LockingClientStoreTests : IDisposable {
        private readonly IClientStore _decorated;
        private readonly AsyncReaderWriterLock _lock;
        private readonly ILockFactory _lockFactory;
        private readonly LockingClientStore _sut;

        public LockingClientStoreTests() {
            FakeFactory.Create(out _decorated, out _lockFactory);
            _lock = new AsyncReaderWriterLock();
            A.CallTo(() => _lockFactory.CreateLock())
                .Returns(_lock);
            _sut = new LockingClientStore(_decorated, _lockFactory);
        }

        public void Dispose() {
            _sut?.Dispose();
        }

        public class DisposableSupport : LockingClientStoreTests {
            [Fact]
            public void DisposesDecoratedService() {
                _sut.Dispose();

                A.CallTo(() => _decorated.Dispose())
                    .MustHaveHappened();
            }
        }
        
        public class Register : LockingClientStoreTests {
            [Fact]
            public async Task CallsDecoratedService() {
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

                A.CallTo(() => _decorated.Register(client))
                    .MustHaveHappened();
            }
        }

        public class Get : LockingClientStoreTests {
            [Fact]
            public async Task ReturnsResultFromDecoratedService() {
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

                A.CallTo(() => _decorated.Get(client.Id))
                    .Returns(client);

                var actual = await _sut.Get(client.Id);

                actual.Should().Be(client);
            }
        }
    }
}