using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Validation {
    public class KeyStoreTests {
        private readonly KeyStore _sut;

        public KeyStoreTests() {
            _sut = new KeyStore();
        }

        public class Register : KeyStoreTests {
            [Fact]
            public void WhenEntryIsNull_ThrowsArgumentNullException() {
                Action act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenEntryAlreadyExists_ThrowsInvalidOperationException() {
                var entry = new KeyStoreEntry("entry1", "s3cr3t", SignatureAlgorithm.RSA, HashAlgorithm.SHA256);
                _sut.Register(entry);
                Action act = () => _sut.Register(entry);
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void AddsEntry() {
                var entry = new KeyStoreEntry("entry1", "s3cr3t", SignatureAlgorithm.RSA, HashAlgorithm.SHA256);
                _sut.Register(entry);
                var registeredEntry = _sut.Get(entry.Id);
                registeredEntry.Should().Be(entry);
            }
        }

        public class Get : KeyStoreTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => _sut.Get(nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void WhenItemIsNotFound_ThrowsHttpMessageSigningSignatureValidationException() {
                Action act = () => _sut.Get("IDontExist");
                act.Should().Throw<HttpMessageSigningSignatureValidationException>();
            }

            [Fact]
            public void WhenItemIsFound_ReturnsFoundItem() {
                var entry = new KeyStoreEntry("entry1", "s3cr3t", SignatureAlgorithm.RSA, HashAlgorithm.SHA256);
                _sut.Register(entry);
                var registeredEntry = _sut.Get(entry.Id);
                registeredEntry.Should().Be(entry);
            }
        }
    }
}