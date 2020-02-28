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
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<Exception>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature)TestModels.Signature.Clone();
                _signature.Algorithm = "hmac-sha256";
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Created, _signature.Created.Value.ToUnixTimeSeconds().ToString());
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("hs2019"));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenSignatureIncludesCreatedHeader_ButItShouldNot_ReturnsSignatureVerificationException(string algorithm) {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm(algorithm));
                _signature.Algorithm = algorithm + "-sha256";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenSignatureDoesNotSpecifyACreationTime_ReturnsSignatureVerificationException() {
                _signature.Created = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenCreatedHeaderIsNotAValidTimestamp_ReturnsSignatureVerificationException() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Created] = "{Nonsense}";

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenCreatedHeaderDoesNotMatchSignatureCreation_ReturnsSignatureVerificationException() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Created] = (long.Parse(_signedRequest.Headers[HeaderName.PredefinedHeaderNames.Created]) + 1).ToString();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenCreatedHeaderMatchesSignatureCreation_ReturnsNull() {
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}