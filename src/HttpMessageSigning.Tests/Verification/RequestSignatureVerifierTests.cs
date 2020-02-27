using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class RequestSignatureVerifierTests {
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly IClientStore _clientStore;
        private readonly ISignatureParser _signatureParser;
        private readonly ISignatureSanitizer _signatureSanitizer;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly RequestSignatureVerifier _sut;

        public RequestSignatureVerifierTests() {
            FakeFactory.Create(out _signatureParser, out _clientStore, out _signatureVerifier, out _claimsPrincipalFactory, out _signatureSanitizer);
            _sut = new RequestSignatureVerifier(_signatureParser, _clientStore, _signatureVerifier, _claimsPrincipalFactory, _signatureSanitizer);
        }

        public class VerifySignature : RequestSignatureVerifierTests {
            private readonly DefaultHttpRequest _httpRequest;

            public VerifySignature() {
                _httpRequest = new DefaultHttpRequest(new DefaultHttpContext()) {
                    Method = "POST",
                    Scheme = "https",
                    Host = new HostString("unittest.com", 9000)
                };
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task VerifiesSanitizedSignatureOfClient_ThatMatchesTheKeyIdFromTheRequest() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var sanitizedSignature = new Signature {KeyId = new KeyId("app001"), Headers = new[] {new HeaderName("h1")}};
                A.CallTo(() => _signatureSanitizer.Sanitize(signature, client))
                    .Returns(sanitizedSignature);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((Exception)null);
                
                await _sut.VerifySignature(_httpRequest);

                A.CallTo(() => _signatureVerifier.VerifySignature(
                        A<HttpRequestForSigning>.That.Matches(_ => _.RequestUri == new Uri("https://unittest.com:9000/")), 
                        sanitizedSignature, 
                        client))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenVerificationSucceeds_ReturnsSuccessResultWithClaimsPrincipal() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var principal = new ClaimsPrincipal();
                A.CallTo(() => _claimsPrincipalFactory.CreateForClient(client))
                    .Returns(principal);
                
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((Exception)null);
                
                var actual = await _sut.VerifySignature(_httpRequest);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
                actual.As<RequestSignatureVerificationResultSuccess>().IsSuccess.Should().BeTrue();
                actual.As<RequestSignatureVerificationResultSuccess>().Principal.Should().Be(principal);
            }

            [Fact]
            public async Task WhenVerificationFails_ReturnsFailureResult() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);
                
                var failure = new SignatureVerificationException("Invalid signature.");
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns(failure);

                var actual = await _sut.VerifySignature(_httpRequest);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().SignatureVerificationException.Should().Be(failure);
            }

            [Fact]
            public void WhenVerificationReturnsAnotherException_Rethrows() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);
                
                var failure = new InvalidOperationException("Not something to do with verification.");
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns(failure);

                Func<Task> act = () => _sut.VerifySignature(_httpRequest);
                act.Should().Throw<InvalidOperationException>().Where(ex => ex == failure);
            }
            
            [Fact]
            public async Task WhenSignatureCannotBeParsed_ReturnsFailureResult() {
                var failure = new SignatureVerificationException("Cannot parse signature.");
                A.CallTo(() => _signatureParser.Parse(_httpRequest))
                    .Throws(failure);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((Exception)null);
                
                var actual = await _sut.VerifySignature(_httpRequest);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().SignatureVerificationException.Should().Be(failure);
            }

            [Fact]
            public async Task WhenClientDoesNotExist_ReturnsFailureResult() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_httpRequest))
                    .Returns(signature);

                var failure = new SignatureVerificationException("Don't know that client.");
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Throws(failure);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._))
                    .Returns((Exception)null);
                
                var actual = await _sut.VerifySignature(_httpRequest);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().SignatureVerificationException.Should().Be(failure);
            }
        }
    }
}