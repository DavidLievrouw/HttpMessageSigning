using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class PooledHMACPolicyTests {
        private readonly PooledHMACPolicy _sut;
        private bool _hasBeenCalled;

        public PooledHMACPolicyTests() {
            var key = new byte[] {0, 1, 2, 3};
            _sut = new PooledHMACPolicy(() => {
                _hasBeenCalled = true;
                return new HMACMD5(key);
            });
        }

        public class Create : PooledHMACPolicyTests {
            [Fact]
            public void InvokesCreatorAndReturnsResult() {
                _hasBeenCalled.Should().BeFalse();

                var actual = _sut.Create();

                _hasBeenCalled.Should().BeTrue();
                actual.Should().NotBeNull().And.BeAssignableTo<HMACMD5>();
            }
        }

        public class Return : PooledHMACPolicyTests {
            private readonly HMAC _poolItem;

            public Return() {
                _poolItem = _sut.Create();
            }

            [Fact]
            public void ReturnsTrue() {
                var actual = _sut.Return(_poolItem);
                actual.Should().BeTrue();
            }
        }
    }
}