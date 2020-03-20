using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<Exception>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForSigning) TestModels.Request.Clone();
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.MD5), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Fact]
            public async Task WhenAlgorithmIsNotSpecifiedInSignature_ReturnsNull() {
                _signature.Algorithm = null;
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenAlgorithmDoesNotConsistOfSignatureAndHash_ReturnsSignatureVerificationException() {
                _signature.Algorithm = "hmac";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }
            
            [Fact]
            public async Task WhenSignatureAlgorithmDoesNotMatch_ReturnsSignatureVerificationException() {
                _signature.Algorithm = "custom-md5";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }
            
            [Fact]
            public async Task WhenHashAlgorithmDoesNotMatch_ReturnsSignatureVerificationException() {
                _signature.Algorithm = "hmac-sha256";
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
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