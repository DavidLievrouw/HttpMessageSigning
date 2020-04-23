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
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }
            
            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenCreatedHeaderIsTakenIntoAccount_ButItShouldNot_ReturnsSignatureVerificationException(string algorithm) {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm(algorithm), TimeSpan.FromMinutes(1));
                _signature.Algorithm = algorithm + "-sha256";
                _signature.Created = DateTimeOffset.UtcNow;

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }
            
            [Fact]
            public async Task WhenCreatedHeaderIsNotTakenIntoAccount_ButItShould_ReturnsNull_BecauseItIsOnlyARecommendation() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Algorithm = "hs2019";
                _signature.Created = DateTimeOffset.UtcNow;
                _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Created).ToArray();
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenCreatedHeaderHasNoValue_ButItIsRequiredByTheClient_ReturnsSignatureVerificationFailure() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Created = null;
                _signature.Algorithm = "hs2019";
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }
            
            [Fact]
            public async Task WhenCreatedHeaderHasNoValue_AndItIsNotRequiredByTheClient_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Created = null;
                _signature.Algorithm = "hs2019";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenCreatedHeaderHasAValue_AndItIsNotRequiredByTheClient_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Created = DateTimeOffset.UtcNow;
                _signature.Algorithm = "hs2019";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenCreatedHeaderHasAValue_AndItIsRequiredByTheClient_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Created = DateTimeOffset.UtcNow;
                _signature.Algorithm = "hs2019";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
        }
    }
}