using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class ExpiresHeaderGuardVerificationTaskTests {
        private readonly ExpiresHeaderGuardVerificationTask _sut;

        public ExpiresHeaderGuardVerificationTaskTests() {
            _sut = new ExpiresHeaderGuardVerificationTask();
        }

        public class Verify : ExpiresHeaderGuardVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature)TestModels.Signature.Clone();
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }
            
            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenExpiresHeaderIsTakenIntoAccount_ButItShouldNot_ReturnsSignatureVerificationException(string algorithm) {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm(algorithm), TimeSpan.FromMinutes(1));
                _signature.Algorithm = algorithm + "-sha256";
                _signature.Expires = DateTimeOffset.UtcNow;

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_EXPIRES_HEADER");
            }
            
            [Fact]
            public async Task WhenExpiresHeaderIsNotTakenIntoAccount_ButItShould_ReturnsNull_BecauseItIsOnlyARecommendation() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Algorithm = "hs2019";
                _signature.Expires = DateTimeOffset.UtcNow;
                _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Expires).ToArray();
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenExpiresHeaderHasNoValue_ButItIsRequiredByTheClient_ReturnsSignatureVerificationFailure() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Expires = null;
                _signature.Algorithm = "hs2019";
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_EXPIRES_HEADER");
            }
            
            [Fact]
            public async Task WhenExpiresHeaderHasNoValue_AndItIsNotRequiredByTheClient_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Expires = null;
                _signature.Algorithm = "hs2019";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenExpiresHeaderHasAValue_AndItIsNotRequiredByTheClient_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Expires = DateTimeOffset.UtcNow;
                _signature.Algorithm = "hs2019";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenExpiresHeaderHasAValue_AndItIsRequiredByTheClient_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Expires = DateTimeOffset.UtcNow;
                _signature.Algorithm = "hs2019";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
        }
    }
}