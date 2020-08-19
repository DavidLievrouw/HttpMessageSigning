using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class PooledHashAlgorithmPolicyTests {
        private readonly PooledHashAlgorithmPolicy _sut;
        private bool _hasBeenCalled;

        public PooledHashAlgorithmPolicyTests() {
            _sut = new PooledHashAlgorithmPolicy(() => {
                _hasBeenCalled = true;
                return new SHA256Managed();
            });
        }

        public class Create : PooledHashAlgorithmPolicyTests {
            [Fact]
            public void InvokesCreatorAndReturnsResult() {
                _hasBeenCalled.Should().BeFalse();

                var actual = _sut.Create();

                _hasBeenCalled.Should().BeTrue();
                actual.Should().NotBeNull().And.BeAssignableTo<SHA256Managed>();
            }
        }

        public class Return : PooledHashAlgorithmPolicyTests {
            private readonly HashAlgorithm _poolItem;

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