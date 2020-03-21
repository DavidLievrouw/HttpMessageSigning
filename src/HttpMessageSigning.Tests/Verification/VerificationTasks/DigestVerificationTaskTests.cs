using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class DigestVerificationTaskTests {
        private readonly IBase64Converter _base64Converter;
        private readonly ILogger<DigestVerificationTask> _logger;
        private readonly DigestVerificationTask _sut;

        public DigestVerificationTaskTests() {
            FakeFactory.Create(out _logger);
            _base64Converter = new Base64Converter();
            _sut = new DigestVerificationTask(_base64Converter, _logger);
        }

        public class Verify : DigestVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForSigning) TestModels.Request.Clone();
                _client = (Client) TestModels.Client.Clone();
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
                
                _signedRequest.Body = Encoding.UTF8.GetBytes("I am the body payload");
                using (var hashAlgorithm = HashAlgorithm.Create("SHA-384")) {
                    var digestBytes = hashAlgorithm.ComputeHash(_signedRequest.Body);
                    var digestString = new Base64Converter().ToBase64(digestBytes);
                    _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Digest, "SHA-384=" + digestString);
                }

                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Digest}).ToArray();
            }

            [Fact]
            public async Task WhenDigestHeaderIsNotSpecified_ButItIsIncludedInTheSignature_ReturnsSignatureVerificationFailure() {
                _signedRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("HEADER_MISSING");
            }

            [Fact]
            public async Task WhenDigestHeaderIsNotSpecified_AndItIsNotIncludedInTheSignature_ReturnsNull() {
                _signedRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Digest).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenDigestHeaderIsSpecified_AndTheBodyIsNull_ReturnsSignatureVerificationFailure() {
                _signedRequest.Body = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_DIGEST_HEADER");
            }

            [Fact]
            public async Task WhenDigestHeaderIsNonsense_ReturnsSignatureVerificationFailure() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Digest] = "{nonsense}";

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_DIGEST_HEADER");
            }

            [Fact]
            public async Task WhenDigestHeaderAlgorithmIsNotSupported_ReturnsSignatureVerificationFailure() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Digest] = "Custom=abc123";

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_DIGEST_HEADER");
            }

            [Fact]
            public async Task WhenDigestHeaderDoesNotMatchCalculatedBodyDigest_ReturnsSignatureVerificationFailure() {
                using (var hashAlgorithm = HashAlgorithm.Create("SHA-384")) {
                    var digestBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(_signedRequest.Body + "a"));
                    var digestString = new Base64Converter().ToBase64(digestBytes);
                    _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Digest] = "SHA-384=" + digestString;
                }

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_DIGEST_HEADER");
            }

            [Fact]
            public async Task WhenDigestHeaderMatchesCalculatedBodyDigest_ReturnsNull() {
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}