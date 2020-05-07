using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.HttpMessages;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verify {
    public class Tests {
        private readonly DateTimeOffset _now;
        private readonly VerifyOptions _options;
        private readonly Action<Signature> _setKeyId;
        
        public Tests() {
            _now = DateTimeOffset.Now;

            _setKeyId = signature => signature.KeyId = new KeyId("test");
            _options = new VerifyOptions {
                Message = HttpMessageGenerator
                    .GenerateMessage("rsa-signed-request", _now)
                    .ToServerSideHttpRequest().GetAwaiter().GetResult(),
                PublicKey = "rsa.pub",
                Headers = "host,digest",
                KeyType = "rsa",
                ModifyParsedSignature = (request, signature) => {
                    _setKeyId(signature);
                    return Task.CompletedTask;
                }
            };
        }

        [Fact]
        public async Task CanVerifyRSASignature() {
            _options.Message = await HttpMessageGenerator
                .GenerateMessage("rsa-signed-request", _now)
                .ToServerSideHttpRequest();
            _options.KeyType = "rsa";
            _options.PublicKey = "rsa.pub";

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanVerifyECDSASignature() {
            _options.Message = await HttpMessageGenerator
                .GenerateMessage("ecdsa-signed-request", _now)
                .ToServerSideHttpRequest();
            _options.KeyType = "ecdsa";
            _options.PublicKey = "koblitzCurve.pub";

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanVerifyP256Signature() {
            _options.Message = await HttpMessageGenerator
                .GenerateMessage("p256-signed-request", _now)
                .ToServerSideHttpRequest();
            _options.KeyType = "ecdsa";
            _options.PublicKey = "p256.pub";
            
            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CanVerifyHMACSignature() {
            _options.Message = await HttpMessageGenerator
                .GenerateMessage("hmac-signed-request", _now)
                .ToServerSideHttpRequest();
            _options.KeyType = "hmac";
            _options.PublicKey = "s3cr37";
            
            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task WhenSignatureAlgorithmDoesNotMatchKeyMetadata_FailsVerification() {
            _options.ModifyParsedSignature = (request, signature) => {
                _setKeyId(signature);
                signature.Algorithm = "hmac-sha256";
                return Task.CompletedTask;
            };

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task WhenSignatureIsMissing_FailsVerification() {
            _options.ModifyParsedSignature = (request, signature) => {
                _setKeyId(signature);
                signature.String = null;
                return Task.CompletedTask;
            };

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task WhenKeyIdIsMissing_FailsVerification() {
            _options.ModifyParsedSignature = (request, signature) => {
                signature.KeyId = KeyId.Empty;
                return Task.CompletedTask;
            };

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task WhenAHeaderIsPartOfTheSignature_ButIsNotInTheRequest_FailsVerification() {
            _options.ModifyParsedSignature = (request, signature) => {
                _setKeyId(signature);
                signature.Headers = signature.Headers.Concat(new[] {new HeaderName("not-in-request")}).ToArray();
                return Task.CompletedTask;
            };

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task WhenSignatureAlgorithmIsUnsupported_FailsVerification() {
            _options.ModifyParsedSignature = (request, signature) => {
                _setKeyId(signature);
                signature.Algorithm = "unsupported";
                return Task.CompletedTask;
            };

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeFalse();
        }
        
        [Theory]
        [InlineData("rsa-sha1")]
        [InlineData("rsa-sha256")]
        [InlineData("hmac-sha256")]
        [InlineData("ecdsa-sha512")]
        public async Task WhenSignatureAlgorithmIsDeprecated_FailsVerification(string deprecatedAlgorithm) {
            _options.ModifyParsedSignature = (request, signature) => {
                _setKeyId(signature);
                signature.Algorithm = deprecatedAlgorithm;
                return Task.CompletedTask;
            };

            var isSuccess = await Verifier.Run(_options);

            isSuccess.Should().BeFalse();
        }
    }
}