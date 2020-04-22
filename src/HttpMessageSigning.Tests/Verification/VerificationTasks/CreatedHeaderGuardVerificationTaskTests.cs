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
                _signature.Algorithm = "hs2019";
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("hs2019"), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Fact]
            public async Task WhenCreatedHeaderIsMissing_ButItIsRequired_ReturnsSignatureVerificationFailure() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Created = null;
                
                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }
            
            [Fact]
            public async Task WhenCreatedHeaderIsMissing_AndItIsNotRequired_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Created = null;

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenCreatedHeaderIsPresent_AndItIsNotRequired_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("RSA"), TimeSpan.FromMinutes(1));
                _signature.Created = DateTimeOffset.UtcNow;

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenCreatedHeaderIsPresent_AndItIsRequired_ReturnsNull() {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm("CUSTOM"), TimeSpan.FromMinutes(1));
                _signature.Created = DateTimeOffset.UtcNow;

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().BeNull();
            }
        }
    }
}