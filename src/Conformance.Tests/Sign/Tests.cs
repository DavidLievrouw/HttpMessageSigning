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

        [Fact]
        public void WhenAnUnsupportedAlgorithmIsSpecified_ProducesError() {
            _options.Algorithm = "unknown";

            Func<Task> act = () => Signer.Run(_options);

            act.Should().Throw<Exception>();
        }

        [Theory]
        [InlineData("rsa-sha1")]
        [InlineData("rsa-sha256")]
        [InlineData("hmac-sha256")]
        [InlineData("ecdsa-sha512")]
        public async Task DoesNotReportDeprecatedAlgorithms(string deprecatedAlgorithm) {
            _options.Algorithm = deprecatedAlgorithm;

            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            signedMessage.Headers.Authorization.Parameter.Should().Contain("algorithm=\"hs2019\"");
        }

        [Fact]
        public async Task SignsWithSpecifiedAlgorithm() {
            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            signedMessage.Headers.Authorization.Parameter.Should().Contain("algorithm=\"hs2019\"");
        }

        [Fact]
        public async Task SignsWithExpectedAuthorizationScheme() {
            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            signedMessage.Headers.Authorization.Scheme.Should().Be("Signature");
        }

        [Fact]
        public void WhenCreatingASignatureInTheFuture_ProducesError() {
            _options.Created = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds().ToString();

            Func<Task> act = () => Signer.Run(_options);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void WhenCreatingAnExpiredSignature_ProducesError() {
            _options.Expires = DateTimeOffset.Now.AddMinutes(-10).ToUnixTimeSeconds().ToString();

            Func<Task> act = () => Signer.Run(_options);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task CanSignWithAnRSAKey() {
            _options.KeyType = "rsa";
            _options.PrivateKey = "rsa.private";

            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            Base64SignatureRegex.IsMatch(signedMessage.Headers.Authorization.Parameter).Should().BeTrue();
        }

        [Fact]
        public async Task CanSignWithAnHMACKey() {
            _options.KeyType = "hmac";
            _options.PrivateKey = "s3cr37";

            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            Base64SignatureRegex.IsMatch(signedMessage.Headers.Authorization.Parameter).Should().BeTrue();
        }

        [Fact]
        public async Task CanSignWithAnECDSAKey() {
            _options.KeyType = "ecdsa";
            _options.PrivateKey = "koblitzCurve.private";

            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            Base64SignatureRegex.IsMatch(signedMessage.Headers.Authorization.Parameter).Should().BeTrue();
        }

        [Fact]
        public async Task CanSignWithAnP256Key() {
            _options.KeyType = "ecdsa";
            _options.PrivateKey = "P256.private";

            var signedMessage = await Signer.Run(_options).ToHttpRequestMessage();

            Base64SignatureRegex.IsMatch(signedMessage.Headers.Authorization.Parameter).Should().BeTrue();
        }
    }
}