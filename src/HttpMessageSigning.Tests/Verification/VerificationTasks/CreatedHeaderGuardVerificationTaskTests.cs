using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class CreatedHeaderGuardVerificationTaskTests {
        private readonly CreatedHeaderGuardVerificationTask _sut;

        public CreatedHeaderGuardVerificationTaskTests() {
            _sut = new CreatedHeaderGuardVerificationTask();
        }

        public class Verify : CreatedHeaderGuardVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature)TestModels.Signature.Clone();
                _signature.Algorithm = "hmac-sha256";
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName(), _signature.Created.Value.ToUnixTimeSeconds().ToString());
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("hs2019"), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenSignatureIncludesCreatedHeader_ButItShouldNot_IsStillOk_ReturnsNull(string algorithm) {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm(algorithm), TimeSpan.FromMinutes(1));
                _signature.Algorithm = algorithm + "-sha256";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenCreatedHeaderIsMissing_ButItIsRequired_ReturnsSignatureVerificationFailure() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("hs2019"), TimeSpan.FromMinutes(1));
                _signature.Algorithm = "hs2019-sha256";
                _signedRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName());
                _signature.Created = null;
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }
            
            [Fact]
            public async Task WhenSignatureDoesNotSpecifyACreationTime_ReturnsSignatureVerificationFailure() {
                _signature.Created = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }

            [Fact]
            public async Task WhenCreatedHeaderIsNotAValidTimestamp_ReturnsSignatureVerificationFailure() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName()] = "{Nonsense}";

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }

            [Fact]
            public async Task WhenCreatedHeaderDoesNotMatchSignatureCreation_ReturnsSignatureVerificationFailure() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName()] = (long.Parse(_signedRequest.Headers[HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName()]) + 1).ToString();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }

            [Fact]
            public async Task WhenCreatedHeaderMatchesSignatureCreation_ReturnsNull() {
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task SupportsCreatedHeaderFromSpec() {
                // See https://stackoverflow.com/a/51039555
                _signedRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName());
                _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Created, _signature.Created.Value.ToUnixTimeSeconds().ToString());
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}