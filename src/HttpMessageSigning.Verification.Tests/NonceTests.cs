using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class NonceTests {
        public class Construction : NonceTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAllowNullOrEmptyValue(string nullOrEmpty) {
                // ReSharper disable once ObjectCreationAsStatement
                Action act = () => new Nonce(new KeyId("abc"), nullOrEmpty, DateTimeOffset.Now);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAllowEmptyKeyId() {
                // ReSharper disable once ObjectCreationAsStatement
                Action act = () => new Nonce(KeyId.Empty, "abc123", DateTimeOffset.Now);
                act.Should().Throw<ArgumentException>();
            }
        }
    }
}