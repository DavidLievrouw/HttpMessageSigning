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
                _signature.Algorithm = "hs2019";
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("hs2019"), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Fact]
            public async Task WhenExpiresHeaderIsMissing_ButItIsRequired_ReturnsSignatureVerificationFailure() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Expires = null;
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_EXPIRES_HEADER");
            }
            
            [Fact]
            public async Task WhenExpiresHeaderIsMissing_AndItIsNotRequired_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Expires = null;

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenExpiresHeaderIsPresent_AndItIsNotRequired_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Expires = DateTimeOffset.UtcNow.AddMinutes(1);

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenExpiresHeaderIsPresent_AndItIsRequired_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Expires = DateTimeOffset.UtcNow.AddMinutes(1);

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
        }
    }
}