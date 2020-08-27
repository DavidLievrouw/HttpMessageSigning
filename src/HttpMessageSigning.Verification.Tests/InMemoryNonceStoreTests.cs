﻿using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class InMemoryNonceStoreTests {
        private readonly FakeMemoryCache _cache;
        private readonly InMemoryNonceStore _sut;

        public InMemoryNonceStoreTests() {
            _cache = new FakeMemoryCache();
            _sut = new InMemoryNonceStore(_cache);
        }

        public class Get : InMemoryNonceStoreTests {
            private readonly string _value;
            private readonly KeyId _keyId;

            public Get() {
                _keyId = new KeyId("c1");
                _value = "abc123";
            }

            [Fact]
            public void GivenEmptyKeyId_ThrowsArgumentException() {
                Func<Task> act = () => _sut.Get(KeyId.Empty, _value);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyNonceValue_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(_keyId, nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public async Task WhenNonceIsRegistered_ReturnsNonce() {
                var nonce = new Nonce(_keyId, _value, DateTimeOffset.Now.AddMinutes(1));
                var cacheKey = "Nonce_c1_abc123";
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = nonce;

                var actual = await _sut.Get(_keyId, _value);

                actual.Should().Be(nonce);
            }

            [Fact]
            public async Task WhenNonceIsRegistered_ReturnsNonce_EvenIfItExpired() {
                var nonce = new Nonce(_keyId, _value, DateTimeOffset.Now.AddMinutes(-1));
                var cacheKey = "Nonce_c1_abc123";
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = nonce;

                var actual = await _sut.Get(_keyId, _value);

                actual.Should().Be(nonce);
            }

            [Fact]
            public async Task WhenNonceIsNotRegistered_ReturnsNull() {
                var actual = await _sut.Get(_keyId, _value);

                actual.Should().BeNull();
            }
        }

        public class Register : InMemoryNonceStoreTests {
            private readonly DateTimeOffset _now;
            private readonly Nonce _nonce;
            private readonly string _cacheKey;

            public Register() {
                _now = new DateTimeOffset(2020, 3, 20, 12, 12, 14, TimeSpan.Zero);
                _cacheKey = "Nonce_c1_abc123";
                _nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddMinutes(1));
            }

            [Fact]
            public void GivenNullNonce_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WhenNonceIsNotRegistered_RegistersNonce() {
                _cache.TryGetEntry(_cacheKey, out _).Should().BeFalse();

                await _sut.Register(_nonce);

                _cache.TryGetValue(_cacheKey, out var actualNonce).Should().BeTrue();
                actualNonce.Should().Be(_nonce);
            }

            [Fact]
            public async Task WhenNonceIsNotRegistered_RegistersNonceWithExpectedKey() {
                _cache.TryGetEntry(_cacheKey, out _).Should().BeFalse();

                await _sut.Register(_nonce);

                _cache.TryGetEntry(_cacheKey, out _).Should().BeTrue();
            }

            [Fact]
            public async Task WhenNonceIsNotRegistered_RegistersNonceWithExpectedExpiration() {
                _cache.TryGetEntry(_cacheKey, out _).Should().BeFalse();

                await _sut.Register(_nonce);

                _cache.TryGetEntry(_cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(_nonce.Expiration);
            }

            [Fact]
            public async Task WhenNonceIsAlreadyRegistered_Overwrites() {
                var cacheKey = "Nonce_c1_abc123";
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = _nonce;

                var newNonce = new Nonce(_nonce.ClientId, _nonce.Value, _now.AddMinutes(10));
                await _sut.Register(newNonce);

                _cache.TryGetValue(_cacheKey, out var actualNonce).Should().BeTrue();
                actualNonce.Should().Be(newNonce);
            }
        }
    }
}