using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Utils {
    public class StringProtectorFactoryTests {
        private readonly StringProtectorFactory _sut;

        public StringProtectorFactoryTests() {
            _sut = new StringProtectorFactory();
        }

        public class CreateSymmetric : StringProtectorFactoryTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptySecret_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => _sut.CreateSymmetric(nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void CreatesNewInstanceOfExpectedType() {
                var actual = _sut.CreateSymmetric("s3cr3t");
                
                actual.Should().NotBeNull();
                actual.Should().BeAssignableTo<SymmetricStringProtector>();
            }
        }
    }
}