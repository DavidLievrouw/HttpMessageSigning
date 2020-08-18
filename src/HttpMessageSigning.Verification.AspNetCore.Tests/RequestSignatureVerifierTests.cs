using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class RequestSignatureVerifierTests {
        private readonly IRequestSignatureVerificationOrchestrator _requestSignatureVerificationOrchestrator;
        private readonly ISignatureParser _signatureParser;
        private readonly ILogger<RequestSignatureVerifier> _logger;
        private readonly RequestSignatureVerifier _sut;

        public RequestSignatureVerifierTests() {
            FakeFactory.Create(out _signatureParser, out _requestSignatureVerificationOrchestrator, out _logger);
            _sut = new RequestSignatureVerifier(_signatureParser, _requestSignatureVerificationOrchestrator, _logger);
        }

        public class VerifySignature : RequestSignatureVerifierTests {
            private readonly HttpRequest _httpRequest;
            private readonly SignedRequestAuthenticationOptions _options;
            private readonly Signature _signature;
            private readonly RequestSignatureVerificationResultSuccess _verificationSuccessResult;

            public VerifySignature() {
                _httpRequest = new DefaultHttpContext().Request;
                _httpRequest.Method = "POST";
                _httpRequest.Scheme = "https";
                _httpRequest.Host = new HostString("unittest.com", 9000);
                _options = new SignedRequestAuthenticationOptions();
                _signature = (Signature) TestModels.Signature.Clone();
                _verificationSuccessResult = new RequestSignatureVerificationResultSuccess(
                    new Client(_signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)), 
                    _signature, 
                    new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")})));
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(null, _options);
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Fact]
            public void GivenNullOptions_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(_httpRequest, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WhenSignatureIsParsed_InvokesEventCallback() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(_signature);

                Signature interceptedSignature = null;
                _options.OnSignatureParsed = (request, sig) => {
                    interceptedSignature = sig;
                    return Task.CompletedTask;
                };

                await _sut.VerifySignature(_httpRequest, _options);

                interceptedSignature.Should().Be(_signature);
            }
            
            [Fact]
            public void WhenSignatureIsParsed_AndEventCallbackIsNull_DoesNotThrow() {
                _options.OnSignatureParsed = null;
                
                Func<Task> act = () => _sut.VerifySignature(_httpRequest, _options);
                act.Should().NotThrow();
            }
            
            [Fact]
            public async Task VerifiesUsingSignatureManipulatedByCallback() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(_signature);

                HttpRequestForVerification intercepted = null;
                A.CallTo(() => _requestSignatureVerificationOrchestrator.VerifySignature(A<HttpRequestForVerification>._))
                    .Invokes(call => intercepted = call.GetArgument<HttpRequestForVerification>(0))
                    .Returns(_verificationSuccessResult);
                
                _options.OnSignatureParsed = (request, sig) => {
                    sig.KeyId = new KeyId("xyz");
                    return Task.CompletedTask;
                };

                await _sut.VerifySignature(_httpRequest, _options);

                var expectedSignature = (Signature)_signature.Clone();
                expectedSignature.KeyId = new KeyId("xyz");
                intercepted.Should().NotBeNull();
                intercepted.Signature.Should().NotBeNull();
                intercepted.Signature.Should().BeEquivalentTo(expectedSignature);
            }
            
            [Fact]
            public async Task ReturnsVerificationResult() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(_signature);

                A.CallTo(() => _requestSignatureVerificationOrchestrator.VerifySignature(A<HttpRequestForVerification>._))
                    .Returns(_verificationSuccessResult);

                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().Be(_verificationSuccessResult);
            }
            
            [Fact]
            public async Task WhenSignatureCannotBeParsed_ReturnsFailureResult() {
                var failure = new InvalidSignatureException("Cannot parse signature.");
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Throws(failure);
                
                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE");
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Exception.Should().Be(failure);
            }
        }

        public class Dispose : RequestSignatureVerifierTests {
            [Fact]
            public void DisposeClientStore() {
                _sut.Dispose();

                A.CallTo(() => _requestSignatureVerificationOrchestrator.Dispose())
                    .MustHaveHappened();
            }
        }
    }
}