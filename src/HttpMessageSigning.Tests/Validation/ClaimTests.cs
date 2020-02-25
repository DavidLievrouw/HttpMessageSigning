using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Validation {
    public class ClaimTests {
        public class Construction : ClaimTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyType_ThrowsArgumentException(string nullOrEmpty) {
                // ReSharper disable once ObjectCreationAsStatement
                Action act = () => new Claim(nullOrEmpty, "v1");
                act.Should().Throw<ArgumentException>();
            }
        }
    }
}