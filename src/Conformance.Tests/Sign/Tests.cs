using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.HttpMessages;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Sign {
    public class Tests {
        private readonly DateTimeOffset _now;
        private readonly SignOptions _options;
        private static readonly Regex Base64SignatureRegex = new Regex(@"signature=[""'][a-z0-9=\/\+]+[""']", RegexOptions.IgnoreCase);

        public Tests() {
            _now = DateTimeOffset.Now;
            _options = new SignOptions {
                Message = HttpMessageGenerator.GenerateMessage("default-request", _now),
                PrivateKey = "rsa.private",
                Headers = "digest",
                KeyType = "rsa",
                KeyId = "test"
            };
        }

        [Fact]
        public async Task ShouldGenerateABase64EncodedSignatureString() {
            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();
            
            Base64SignatureRegex.IsMatch(signedMessage.Headers.Authorization.Parameter).Should().BeTrue();
        }
    }
}