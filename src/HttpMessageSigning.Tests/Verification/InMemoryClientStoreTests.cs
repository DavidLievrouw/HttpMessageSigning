using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class InMemoryClientStoreTests {
        private readonly InMemoryClientStore _sut;

        public InMemoryClientStoreTests() {
            _sut = new InMemoryClientStore();
        }

        public class Register : InMemoryClientStoreTests {
            [Fact]
            public void WhenEntryIsNull_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenEntryAlreadyExists_ThrowsInvalidOperationException() {
                var entry = new Client((KeyId) "entry1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), new Claim("c1", "v1"));
                _sut.Register(entry);
                Func<Task> act = () => _sut.Register(entry);
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public async Task AddsEntry() {
                var entry = new Client((KeyId) "entry1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), new Claim("c1", "v1"));
                await _sut.Register(entry);
                var registeredEntry = await _sut.Get(entry.Id);
                registeredEntry.Should().Be(entry);
            }
        }

        public class Get : InMemoryClientStoreTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void WhenItemIsNotFound_ThrowsInvalidClientException() {
                Func<Task> act = () => _sut.Get("IDontExist");
                act.Should().Throw<InvalidClientException>();
            }

            [Fact]
            public async Task WhenItemIsFound_ReturnsFoundItem() {
                var entry = new Client((KeyId) "entry1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), new Claim("c1", "v1"));
                await _sut.Register(entry);
                var registeredEntry = await _sut.Get(entry.Id);
                registeredEntry.Should().Be(entry);
            }
        }

        public class Dispose : InMemoryClientStoreTests {
            [Fact]
            public void DisposesAllEntries() {
                var entries = new[] {
                    new Client((KeyId) "entry1", "Unit test app 1", A.Fake<ISignatureAlgorithm>(), TimeSpan.FromMinutes(1), new Claim("c1", "v1")),
                    new Client((KeyId) "entry2", "Unit test app 2", A.Fake<ISignatureAlgorithm>(), TimeSpan.FromMinutes(1), new Claim("c1", "v1"))
                };
                
                var sut = new InMemoryClientStore(entries);
                sut.Dispose();

                foreach (var client in entries) {
                    A.CallTo(() => client.SignatureAlgorithm.Dispose())
                        .MustHaveHappened();
                }
            }

            [Fact]
            public void ClearsEntries() {
                var entries = new[] {
                    new Client((KeyId) "entry1", "Unit test app 1", A.Fake<ISignatureAlgorithm>(), TimeSpan.FromMinutes(1), new Claim("c1", "v1")),
                    new Client((KeyId) "entry2", "Unit test app 2", A.Fake<ISignatureAlgorithm>(), TimeSpan.FromMinutes(1), new Claim("c1", "v1"))
                };
                
                var sut = new InMemoryClientStore(entries);
                sut.Dispose();

                Func<Task> act = () => sut.Get("entry1");
                act.Should().Throw<InvalidClientException>();
            }
        }
    }
}