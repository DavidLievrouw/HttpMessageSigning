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

        public class Construction : InMemoryClientStoreTests {
            [Fact]
            public void AllowsForNullClients() {
                // ReSharper disable once ObjectCreationAsStatement
                Action act = () => new InMemoryClientStore(null);
                act.Should().NotThrow();
            }
            
            [Fact]
            public async Task RegistersSpecifiedClients() {
                var entry = new Client(
                    (KeyId) "entry1",
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986,
                    new Claim("c1", "v1"));
                var sut = new InMemoryClientStore(entry);
                
                var registeredEntry = await sut.Get(entry.Id);
                
                registeredEntry.Should().Be(entry);
            }
        }
        
        public class Register : InMemoryClientStoreTests {
            [Fact]
            public void WhenEntryIsNull_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WhenEntryAlreadyExists_ReplacesValue() {
                var existingEntry = new Client(
                    (KeyId) "entry1", 
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986, 
                    new Claim("c1", "v1"));
                await _sut.Register(existingEntry);
                
                var updatedEntry = new Client(
                    (KeyId) "entry1", 
                    "Unit test app - updated",
                    new HMACSignatureAlgorithm("s3cr3t_002", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(2),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986, 
                    new Claim("c1", "v2"));
                await _sut.Register(updatedEntry);
                
                var registeredEntry = await _sut.Get(existingEntry.Id);
                
                registeredEntry.Should().Be(updatedEntry);
            }

            [Fact]
            public async Task AddsEntry() {
                var entry = new Client(
                    (KeyId) "entry1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986, 
                    new Claim("c1", "v1"));
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
            public void WhenItemIsNotFound_ReturnsNull() {
                Client actual = null;
                Func<Task> act = async () => actual = await _sut.Get("IDontExist");
                act.Should().NotThrow();
                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenItemIsFound_ReturnsFoundItem() {
                var entry = new Client(
                    (KeyId) "entry1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986, 
                    new Claim("c1", "v1"));
                await _sut.Register(entry);
                var registeredEntry = await _sut.Get(entry.Id);
                registeredEntry.Should().Be(entry);
            }
        }

        public class Dispose : InMemoryClientStoreTests {
            [Fact]
            public void DisposesAllEntries() {
                var entries = new[] {
                    new Client(
                        (KeyId) "entry1", 
                        "Unit test app 1",
                        A.Fake<ISignatureAlgorithm>(), 
                        TimeSpan.FromMinutes(1), 
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC3986, 
                        new Claim("c1", "v1")),
                    new Client(
                        (KeyId) 
                        "entry2", 
                        "Unit test app 2",
                        A.Fake<ISignatureAlgorithm>(),
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC3986, 
                        new Claim("c1", "v1"))
                };
                
                var sut = new InMemoryClientStore(entries);
                sut.Dispose();

                foreach (var client in entries) {
                    A.CallTo(() => client.SignatureAlgorithm.Dispose())
                        .MustHaveHappened();
                }
            }

            [Fact]
            public async Task ClearsEntries() {
                var entries = new[] {
                    new Client(
                        (KeyId) "entry1", 
                        "Unit test app 1", 
                        A.Fake<ISignatureAlgorithm>(), 
                        TimeSpan.FromMinutes(1), 
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC3986, 
                        new Claim("c1", "v1")),
                    new Client(
                        (KeyId) "entry2", 
                        "Unit test app 2", 
                        A.Fake<ISignatureAlgorithm>(),
                        TimeSpan.FromMinutes(1), 
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC3986, 
                        new Claim("c1", "v1"))
                };
                
                var sut = new InMemoryClientStore(entries);
                sut.Dispose();

                var actual = await sut.Get("entry1");
                actual.Should().BeNull();
            }
        }
    }
}