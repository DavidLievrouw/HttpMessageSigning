using System;
using System.Threading.Tasks;
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
                var entry = new Client((KeyId)"entry1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithm.SHA256), new Claim { Type = "c1", Value = "v1" });
                _sut.Register(entry);
                Func<Task> act = () => _sut.Register(entry);
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public async Task AddsEntry() {
                var entry = new Client((KeyId)"entry1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithm.SHA256), new Claim { Type = "c1", Value = "v1" });
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
            public void WhenItemIsNotFound_ThrowsSignatureVerificationException() {
                Func<Task> act = () => _sut.Get("IDontExist");
                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenItemIsFound_ReturnsFoundItem() {
                var entry = new Client((KeyId)"entry1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithm.SHA256), new Claim { Type = "c1", Value = "v1" });
                await _sut.Register(entry);
                var registeredEntry = await _sut.Get(entry.Id);
                registeredEntry.Should().Be(entry);
            }
        }
    }
}