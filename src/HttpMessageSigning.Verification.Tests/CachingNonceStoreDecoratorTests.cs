using System;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class CachingNonceStoreDecoratorTests {
        private readonly IMemoryCache _cache;
        private readonly CachingNonceStoreDecorator _sut;

        public CachingNonceStoreDecoratorTests() {
            FakeFactory.Create(out _cache);
            _sut = new CachingNonceStoreDecorator(_cache);
        }

        public class DecorateWithCaching : CachingNonceStoreDecoratorTests {
            private readonly INonceStore _decorated;

            public DecorateWithCaching() {
                _decorated = A.Fake<INonceStore>();
            }

            [Fact]
            public void GivenNullDecorated_ThrowsArgumentNullException() {
                Action act = () => _sut.DecorateWithCaching(decorated: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ReturnsInstanceOfExpectedType() {
                var actual = _sut.DecorateWithCaching(_decorated);

                actual.Should().NotBeNull().And.BeAssignableTo<CachingNonceStore>();
            }
        }
    }
}