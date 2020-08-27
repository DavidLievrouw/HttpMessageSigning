using System;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class RequestSignatureVerificationOrchestratorTests {
        private readonly IVerificationResultCreatorFactory _verificationResultCreatorFactory;
        private readonly IClientStore _clientStore;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly ILogger<RequestSignatureVerificationOrchestrator> _logger;
        private readonly RequestSignatureVerificationOrchestrator _sut;

        public RequestSignatureVerificationOrchestratorTests() {
            FakeFactory.Create(out _clientStore, out _signatureVerifier, out _verificationResultCreatorFactory, out _logger);
            _sut = new RequestSignatureVerificationOrchestrator(_clientStore, _signatureVerifier, _verificationResultCreatorFactory, _logger);
        }

        public class VerifySignature : RequestSignatureVerificationOrchestratorTests {
            private readonly HttpRequestForVerification _request;

            public VerifySignature() {
                _request = new HttpRequestForVerification {
                    Method = HttpMethod.Post,
                    RequestUri = "https://unittest.com:9000",
                    Signature = (Signature) TestModels.Signature.Clone()
                };
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(request: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task GivenRequestWithoutSignature_ReturnsFailureResult() {
                _request.Signature = null;
                
                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE");
            }
            
            [Fact]
            public async Task WhenSignatureIsInvalid_ReturnsFailureResult() {
                _request.Signature.KeyId = KeyId.Empty; // Make invalid

                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE");
            }
            
            [Fact]
            public async Task VerifiesSignatureOfClient_ThatMatchesTheKeyIdFromTheRequest() {
                var client = new Client(_request.Signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(_request.Signature.KeyId))
                    .Returns(client);

                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, _request))
                    .Returns(verificationResultCreator);
                    
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForVerification>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                await _sut.VerifySignature(_request);

                A.CallTo(() => _signatureVerifier.VerifySignature(_request, client))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenVerificationSucceeds_ReturnsSuccessResultWithClaimsPrincipal() {
                var client = new Client(_request.Signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(_request.Signature.KeyId))
                    .Returns(client);

                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, _request))
                    .Returns(verificationResultCreator);
                
                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                A.CallTo(() => verificationResultCreator.CreateForSuccess())
                    .Returns(new RequestSignatureVerificationResultSuccess(client, _request, principal));
                
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForVerification>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
                actual.As<RequestSignatureVerificationResultSuccess>().IsSuccess.Should().BeTrue();
                actual.As<RequestSignatureVerificationResultSuccess>().Principal.Should().Be(principal);
            }

            [Fact]
            public async Task WhenVerificationFails_ReturnsFailureResult() {
                var client = new Client(_request.Signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(_request.Signature.KeyId))
                    .Returns(client);
                
                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, _request))
                    .Returns(verificationResultCreator);
                
                var failure = SignatureVerificationFailure.SignatureExpired("Invalid signature.");
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForVerification>._, A<Client>._))
                    .Returns(failure);
                
                A.CallTo(() => verificationResultCreator.CreateForFailure(failure))
                    .Returns(new RequestSignatureVerificationResultFailure(client, _request, failure));
                
                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Should().Be(failure);
            }
            
            [Fact]
            public async Task WhenClientDoesNotExist_ReturnsFailureResult() {
                A.CallTo(() => _clientStore.Get(_request.Signature.KeyId))
                    .Returns((Client) null);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForVerification>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_CLIENT");
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Exception.Should().BeNull();
            }
            
            [Fact]
            public void WhenVerificationReturnsAnUnexpectedException_Rethrows() {
                var client = new Client(_request.Signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(_request.Signature.KeyId))
                    .Returns(client);
                
                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, _request))
                    .Returns(verificationResultCreator);
                
                var failure = new InvalidOperationException("Not something to do with verification.");
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForVerification>._, A<Client>._))
                    .Throws(failure);
                
                Func<Task> act = () => _sut.VerifySignature(_request);
                act.Should().Throw<InvalidOperationException>().Where(ex => ex == failure);
            }
        }

        public class Dispose : RequestSignatureVerificationOrchestratorTests {
            [Fact]
            public void DisposeClientStore() {
                _sut.Dispose();

                A.CallTo(() => _clientStore.Dispose())
                    .MustHaveHappened();
            }
        }
    }
}