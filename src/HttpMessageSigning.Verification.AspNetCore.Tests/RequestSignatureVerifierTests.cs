using System;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
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
            private readonly RequestSignatureVerificationResultSuccess _verificationSuccessResult;
            private readonly HttpRequestForVerification _requestForVerification;

            public VerifySignature() {
                _httpRequest = new DefaultHttpContext().Request;
                _httpRequest.Method = "POST";
                _httpRequest.Scheme = "https";
                _httpRequest.Host = new HostString("unittest.com", 9000);
                _options = new SignedRequestAuthenticationOptions();
                _requestForVerification = new HttpRequestForVerification {
                    Method = HttpMethod.Post,
                    RequestUri = "https://unittest.com:9000".ToUri(),
                    Signature = (Signature) TestModels.Signature.Clone()
                };
                _verificationSuccessResult = new RequestSignatureVerificationResultSuccess(
                    new Client(
                        _requestForVerification.Signature.KeyId, 
                        "Unit test app", 
                        new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                        TimeSpan.FromMinutes(1), 
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC3986), 
                    _requestForVerification, 
                    new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")})));
            }

            [Fact]
            public async Task GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(null, _options);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }
            
            [Fact]
            public async Task GivenNullOptions_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(_httpRequest, null);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }

            [Fact]
            public async Task WhenSignatureIsParsed_InvokesEventCallback() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(new SignatureParsingSuccess(_requestForVerification.Signature));

                Signature interceptedSignature = null;
                _options.OnSignatureParsed = (request, sig) => {
                    interceptedSignature = sig;
                    return Task.CompletedTask;
                };

                await _sut.VerifySignature(_httpRequest, _options);

                interceptedSignature.Should().Be(_requestForVerification.Signature);
            }
            
            [Fact]
            public async Task WhenSignatureIsParsed_AndEventCallbackIsNull_DoesNotThrow() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(new SignatureParsingSuccess(_requestForVerification.Signature));
                
                _options.OnSignatureParsed = null;
                
                Func<Task> act = () => _sut.VerifySignature(_httpRequest, _options);
                await act.Should().NotThrowAsync();
            }
            
            [Fact]
            public async Task VerifiesUsingSignatureManipulatedByCallback() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(new SignatureParsingSuccess(_requestForVerification.Signature));

                HttpRequestForVerification intercepted = null;
                A.CallTo(() => _requestSignatureVerificationOrchestrator.VerifySignature(A<HttpRequestForVerification>._))
                    .Invokes(call => intercepted = call.GetArgument<HttpRequestForVerification>(0))
                    .Returns(_verificationSuccessResult);
                
                _options.OnSignatureParsed = (request, sig) => {
                    sig.KeyId = new KeyId("xyz");
                    return Task.CompletedTask;
                };

                await _sut.VerifySignature(_httpRequest, _options);

                var expectedSignature = (Signature)_requestForVerification.Signature.Clone();
                expectedSignature.KeyId = new KeyId("xyz");
                intercepted.Should().NotBeNull();
                intercepted.Signature.Should().NotBeNull();
                intercepted.Signature.Should().BeEquivalentTo(expectedSignature);
            }
            
            [Fact]
            public async Task ReturnsVerificationResult() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(new SignatureParsingSuccess(_requestForVerification.Signature));

                A.CallTo(() => _requestSignatureVerificationOrchestrator.VerifySignature(A<HttpRequestForVerification>._))
                    .Returns(_verificationSuccessResult);

                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().Be(_verificationSuccessResult);
            }
            
            [Fact]
            public async Task WhenSignatureCannotBeParsed_ReturnsFailureResult() {
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(new SignatureParsingFailure("Epic fail."));
                
                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE");
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