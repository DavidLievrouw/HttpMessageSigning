using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.HttpMessages;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verify {
    public class Tests {
        private readonly DateTimeOffset _now;
        private readonly VerifyOptions _options;

        public Tests() {
            _now = DateTimeOffset.Now;
            _options = new VerifyOptions {
                Message = HttpMessageGenerator
                    .GenerateMessage("rsa-signed-request", _now)
                    .ToServerSideHttpRequest()
                    .GetAwaiter().GetResult(),
                PublicKey = "rsa.pub",
                Headers = "host,digest",
                KeyType = "rsa",
                KeyId = "test"
            };
        }

        [Fact]
        public async Task CanVerifyRSASignedRequest() {
            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeTrue();
        }
    }
}