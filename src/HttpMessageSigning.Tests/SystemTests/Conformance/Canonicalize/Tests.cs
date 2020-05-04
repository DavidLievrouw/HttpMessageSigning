using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SystemTests.Conformance.Canonicalize {
    public class Tests {
        [Fact]
        public async Task CreatesSignatureString() {
            var now = DateTimeOffset.Now;
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", now),
                Headers = "date"
            };

            var actual = await Canonicalizer.Run(options);

            var expectedDateString = now.ToString("R");
            actual.Should().Be($"date: {expectedDateString}");
        }
    }
}