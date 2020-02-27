using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class AllHeadersPresentVerificationTaskTests {
        private readonly AllHeadersPresentVerificationTask _sut;

        public AllHeadersPresentVerificationTaskTests() {
            _sut = new AllHeadersPresentVerificationTask();
        }

        public class Verify : AllHeadersPresentVerificationTaskTests {
            private readonly HttpRequestForSigning _signedRequest;
            private readonly Client _client;
            private readonly Signature _signature;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<Exception>> _method;

            public Verify() {
                _signature = new Signature {
                    KeyId = "app1",
                    Algorithm = "hmac-sha256",
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        (HeaderName) "dalion-app-id"
                    },
                    Created = new DateTimeOffset(2020, 2, 27, 14, 18, 22, TimeSpan.Zero),
                    Expires = new DateTimeOffset(2020, 2, 27, 14, 21, 22, TimeSpan.Zero),
                    String = "xyz123="
                };
                _signedRequest = new HttpRequestForSigning {
                    RequestUri = new Uri("https://dalion.eu/api/rsc1"),
                    Method = HttpMethod.Get,
                    SignatureAlgorithmName = "HMAC",
                    Headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                        {"dalion-app-id", "app-one"},
                        {HeaderName.PredefinedHeaderNames.Date, _signature.Created.Value.ToString("R")}
                    })
                };
                _client = new Client(_signature.KeyId, new CustomSignatureAlgorithm("HMAC"));

                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Fact]
            public async Task WhenSignatureDoesNotContainRequestTarget_ReturnsVerificationException() {
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.RequestTarget)
                    .ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenSignatureShouldContainDateHeader_ButItDoesnt_ReturnsVerificationException(string algorithm) {
                var client = new Client(_client.Id, new CustomSignatureAlgorithm(algorithm));
                _signature.Algorithm = algorithm + "-sha256";
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.Date)
                    .ToArray();

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenSignatureShouldNotContainDateHeader_AndItDoesnt_ReturnsNull() {
                var client = new Client(_client.Id, new CustomSignatureAlgorithm("hs2019"));
                _signature.Algorithm = "hs2019";
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.Date)
                    .Concat(new[] {HeaderName.PredefinedHeaderNames.Created})
                    .Concat(new[] {HeaderName.PredefinedHeaderNames.Expires})
                    .ToArray();
                _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Created, _signature.Created.Value.ToUnixTimeSeconds().ToString());
                _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Expires, _signature.Expires.Value.ToUnixTimeSeconds().ToString());
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureShouldContainCreatedHeader_ButItDoesnt_ReturnsVerificationException() {
                var client = new Client(_client.Id, new CustomSignatureAlgorithm("hs2019"));
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.Created)
                    .ToArray();

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenSignatureShouldNotContainCreatedHeader_AndItDoesnt_ReturnsNull(string algorithm) {
                var client = new Client(_client.Id, new CustomSignatureAlgorithm(algorithm));
                _signature.Algorithm = algorithm + "-sha256";
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.Created)
                    .ToArray();

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureShouldContainExpiresHeader_ButItDoesnt_ReturnsVerificationException() {
                var client = new Client(_client.Id, new CustomSignatureAlgorithm("hs2019"));
                _signature.Algorithm = "hs2019";
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.Expires)
                    .ToArray();

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenSignatureShouldNotContainExpiresHeader_AndItDoesnt_ReturnsNull(string algorithm) {
                var client = new Client(_client.Id, new CustomSignatureAlgorithm(algorithm));
                _signature.Algorithm = algorithm + "-sha256";
                _signature.Headers = _signature.Headers
                    .Where(h => h != HeaderName.PredefinedHeaderNames.Expires)
                    .ToArray();

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenOneOrMoreHeadersIsMissing_ReturnsVerificationException() {
                _signedRequest.Headers.Remove("dalion-app-id");

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenAllRequestedHeadersArePresent_ReturnsNull() {
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}