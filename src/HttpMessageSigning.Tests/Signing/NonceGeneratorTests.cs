using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class NonceGeneratorTests {
        private readonly NonceGenerator _sut;

        public NonceGeneratorTests() {
            _sut = new NonceGenerator();
        }

        public class GenerateNonce : NonceGeneratorTests {
            [Fact]
            public void GeneratesNonEmptyNonNullString() {
                var actual = _sut.GenerateNonce();
                actual.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public void GeneratesUniqueString() {
                var actual1 = _sut.GenerateNonce();
                var actual2 = _sut.GenerateNonce();
                actual1.Should().NotBe(actual2);
            }
        }
    }
}