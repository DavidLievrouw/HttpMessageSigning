using System;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class CachingClientStoreDecoratorTests {
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly IMemoryCache _cache;
        private readonly CachingClientStoreDecorator _sut;

        public CachingClientStoreDecoratorTests() {
            FakeFactory.Create(out _cache, out _backgroundTaskStarter);
            _sut = new CachingClientStoreDecorator(_cache, _backgroundTaskStarter);
        }

        public class DecorateWithCaching : CachingClientStoreDecoratorTests {
            private readonly IClientStore _decorated;
            private readonly TimeSpan _expiration;

            public DecorateWithCaching() {
                _expiration = TimeSpan.FromMinutes(5);
                _decorated = A.Fake<IClientStore>();
            }

            [Fact]
            public void GivenNullDecorated_ThrowsArgumentNullException() {
                Action act = () => _sut.DecorateWithCaching(decorated: null, _expiration);
                act.Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void GivenZeroOrNegativeCacheEntryExpiration_DoesNotThrow(int expirationSeconds) {
                Action act = () => _sut.DecorateWithCaching(_decorated, TimeSpan.FromSeconds(expirationSeconds));
                act.Should().NotThrow();
            }

            [Fact]
            public void ReturnsInstanceOfExpectedType() {
                var actual = _sut.DecorateWithCaching(_decorated, _expiration);

                actual.Should().NotBeNull().And.BeAssignableTo<CachingClientStore>();
            }
        }
    }
}