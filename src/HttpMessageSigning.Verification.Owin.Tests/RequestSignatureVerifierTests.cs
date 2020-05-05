using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class RequestSignatureVerifierTests {
        private readonly IClientStore _clientStore;
        private readonly ISignatureParser _signatureParser;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IVerificationResultCreatorFactory _verificationResultCreatorFactory;
        private readonly ILogger<RequestSignatureVerifier> _logger;
        private readonly RequestSignatureVerifier _sut;

        public RequestSignatureVerifierTests() {
            FakeFactory.Create(out _signatureParser, out _clientStore, out _signatureVerifier, out _verificationResultCreatorFactory, out _logger);
            _sut = new RequestSignatureVerifier(_signatureParser, _clientStore, _signatureVerifier, _verificationResultCreatorFactory, _logger);
        }

        public class VerifySignature : RequestSignatureVerifierTests {
            private readonly IOwinRequest _httpRequest;
            private readonly SignedHttpRequestAuthenticationOptions _options;

            public VerifySignature() {
                _httpRequest = new FakeOwinRequest {
                    Method = "POST", 
                    Scheme = "https", 
                    Host = new HostString("unittest.com:9000")
                };
                _options = new SignedHttpRequestAuthenticationOptions();
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(null, _options);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenOptionsRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(_httpRequest, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WhenSignatureIsParsed_InvokesEventCallback() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(signature);

                Signature interceptedSignature = null;
                _options.OnSignatureParsed = (request, sig) => {
                    interceptedSignature = sig;
                    return Task.CompletedTask;
                };

                await _sut.VerifySignature(_httpRequest, _options);

                interceptedSignature.Should().Be(signature);
            }
            
            [Fact]
            public void WhenSignatureIsParsed_AndEventCallbackIsNull_DoesNotThrow() {
                _options.OnSignatureParsed = null;
                
                Func<Task> act = () => _sut.VerifySignature(_httpRequest, _options);
                act.Should().NotThrow();
            }
            
            [Fact]
            public async Task VerifiesSanitizedSignatureOfClient_ThatMatchesTheKeyIdFromTheRequest() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(signature);

                var client = new Client(signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, signature))
                    .Returns(verificationResultCreator);
                
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                await _sut.VerifySignature(_httpRequest, _options);

                A.CallTo(() => _signatureVerifier.VerifySignature(
                        A<HttpRequestForSigning>.That.Matches(_ => _.RequestUri == "/"), 
                        signature, 
                        client))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenVerificationSucceeds_ReturnsSuccessResultWithClaimsPrincipal() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(signature);

                var client = new Client(signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, signature))
                    .Returns(verificationResultCreator);

                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                A.CallTo(() => verificationResultCreator.CreateForSuccess())
                    .Returns(new RequestSignatureVerificationResultSuccess(client, signature, principal));
                
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
                actual.As<RequestSignatureVerificationResultSuccess>().IsSuccess.Should().BeTrue();
                actual.As<RequestSignatureVerificationResultSuccess>().Principal.Should().Be(principal);
            }

            [Fact]
            public async Task WhenVerificationFails_ReturnsFailureResult() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(signature);

                var client = new Client(signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, signature))
                    .Returns(verificationResultCreator);
                
                var failure = SignatureVerificationFailure.InvalidSignatureString("Invalid signature.", null);
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns(failure);
                
                A.CallTo(() => verificationResultCreator.CreateForFailure(failure))
                    .Returns(new RequestSignatureVerificationResultFailure(client, signature, failure));

                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Should().Be(failure);
            }
            
            [Fact]
            public async Task WhenSignatureCannotBeParsed_ReturnsFailureResult() {
                var failure = new InvalidSignatureException("Cannot parse signature.");
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Throws(failure);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE");
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Exception.Should().Be(failure);
            }

            [Fact]
            public async Task WhenClientDoesNotExist_ReturnsFailureResult() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(signature);

                var failure = new InvalidClientException("Don't know that client.");
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Throws(failure);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((SignatureVerificationFailure)null);
                
                var actual = await _sut.VerifySignature(_httpRequest, _options);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_CLIENT");
                actual.As<RequestSignatureVerificationResultFailure>().Failure.Exception.Should().Be(failure);
            }

            [Fact]
            public void WhenVerificationReturnsAnotherException_Rethrows() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest, _options))
                    .Returns(signature);

                var client = new Client(signature.KeyId, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var verificationResultCreator = A.Fake<IVerificationResultCreator>();
                A.CallTo(() => _verificationResultCreatorFactory.Create(client, signature))
                    .Returns(verificationResultCreator);
                
                var failure = new InvalidOperationException("Not something to do with verification.");
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Throws(failure);

                Func<Task> act = () => _sut.VerifySignature(_httpRequest, _options);
                act.Should().Throw<InvalidOperationException>().Where(ex => ex == failure);
            }
        }
    }
}