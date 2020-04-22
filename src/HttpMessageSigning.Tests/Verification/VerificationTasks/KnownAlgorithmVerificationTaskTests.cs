using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class KnownAlgorithmVerificationTaskTests {
        private readonly ILogger<KnownAlgorithmVerificationTask> _logger;
        private readonly KnownAlgorithmVerificationTask _sut;

        public KnownAlgorithmVerificationTaskTests() {
            FakeFactory.Create(out _logger);
            _sut = new KnownAlgorithmVerificationTask(_logger);
        }

        public class Verify : KnownAlgorithmVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForSigning) TestModels.Request.Clone();
                _client = (Client) TestModels.Client.Clone();
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Fact]
            public async Task WhenAlgorithmIsNotSpecifiedInSignature_ReturnsNull() {
                _signature.Algorithm = null;
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenAlgorithmIsHS2019_ReturnsNull() {
                _signature.Algorithm = "hs2019";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenAlgorithmDoesNotConsistOfSignatureAndHash_ReturnsSignatureVerificationFailure() {
                _signature.Algorithm = "hmac";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_SIGNATURE_ALGORITHM");
            }
            
            [Fact]
            public async Task WhenSignatureAlgorithmIsNotSupported_ReturnsSignatureVerificationFailure() {
                _signature.Algorithm = "custom-sha256";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_SIGNATURE_ALGORITHM");
            }
            
            [Fact]
            public async Task WhenHashAlgorithmIsNotSupported_ReturnsSignatureVerificationFailure() {
                _signature.Algorithm = "rsa-custom";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_SIGNATURE_ALGORITHM");
            }
            
            [Fact]
            public async Task WhenAlgorithmIsKnownAndValid_ReturnsNull() {
                _signature.Algorithm = "rsa-sha512";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}