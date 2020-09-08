using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class MatchingAlgorithmVerificationTaskTests {
        private readonly ILogger<MatchingAlgorithmVerificationTask> _logger;
        private readonly MatchingAlgorithmVerificationTask _sut;

        public MatchingAlgorithmVerificationTaskTests() {
            FakeFactory.Create(out _logger);
            _sut = new MatchingAlgorithmVerificationTask(_logger);
        }

        public class Verify : MatchingAlgorithmVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForVerification, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForVerification _signedRequest;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForVerification) TestModels.RequestForVerification.Clone();
                _client = new Client(
                    TestModels.Client.Id, 
                    TestModels.Client.Name,
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.MD5), 
                    TestModels.Client.NonceLifetime, 
                    TestModels.Client.ClockSkew,
                    TestModels.Client.RequestTargetEscaping, 
                    TestModels.Client.Claims);
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
            public async Task WhenSignatureAlgorithmDoesNotMatch_ReturnsSignatureVerificationFailure() {
                _signature.Algorithm = "custom-md5";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_SIGNATURE_ALGORITHM");
            }
            
            [Fact]
            public async Task WhenHashAlgorithmDoesNotMatch_ReturnsSignatureVerificationFailure() {
                _signature.Algorithm = "hmac-sha256";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_SIGNATURE_ALGORITHM");
            }
            
            [Fact]
            public async Task WhenSignatureAlgorithmMatchesClientAlgorithm_ReturnsNull() {
                _signature.Algorithm = "hmac-md5";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}