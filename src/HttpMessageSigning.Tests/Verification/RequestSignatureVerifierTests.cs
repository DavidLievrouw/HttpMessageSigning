using System;
using System.Net.Http;
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
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly ISignatureSanitizer _signatureSanitizer;
        private readonly RequestSignatureVerifier _sut;

        public RequestSignatureVerifierTests() {
            FakeFactory.Create(out _signatureParser, out _clientStore, out _signatureVerifier, out _claimsPrincipalFactory, out _signatureSanitizer);
            _sut = new RequestSignatureVerifier(_signatureParser, _clientStore, _signatureVerifier, _claimsPrincipalFactory, _signatureSanitizer);
        }

        public class VerifySignature : RequestSignatureVerifierTests {
            private readonly DefaultHttpRequest _request;
            private readonly Uri _expectedUri;

            public VerifySignature() {
                _request = new DefaultHttpRequest(new DefaultHttpContext()) {
                    Scheme = "https",
                    Host = new HostString("unittest.com", 9000)
                };
                _expectedUri = new Uri("https://unittest.com:9000", UriKind.Absolute);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task VerifiesSanitizedSignatureOfClient_ThatMatchesTheKeyIdFromTheRequest() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(A<HttpRequestMessage>.That.Matches(r => r.RequestUri == _expectedUri)))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var sanitizedSignature = new Signature {KeyId = new KeyId("app001"), Headers = new[] {new HeaderName("h1")}};
                A.CallTo(() => _signatureSanitizer.Sanitize(signature, client))
                    .Returns(sanitizedSignature);

                await _sut.VerifySignature(_request);

                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestMessage>.That.Matches(r => r.RequestUri == _expectedUri), sanitizedSignature, client))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenVerificationSucceeds_ReturnsSuccessResultWithClaimsPrincipal() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(A<HttpRequestMessage>.That.Matches(r => r.RequestUri == _expectedUri)))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var principal = new ClaimsPrincipal();
                A.CallTo(() => _claimsPrincipalFactory.CreateForClient(client))
                    .Returns(principal);

                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
                actual.As<RequestSignatureVerificationResultSuccess>().IsSuccess.Should().BeTrue();
                actual.As<RequestSignatureVerificationResultSuccess>().Principal.Should().Be(principal);
            }

            [Fact]
            public async Task WhenVerificationFails_ReturnsFailureResult() {
                var failure = new SignatureVerificationException("Invalid signature.");
                A.CallTo(() => _signatureVerifier.VerifySignature(A<HttpRequestMessage>._, A<Signature>._, A<Client>._))
                    .Throws(failure);

                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().SignatureVerificationException.Should().Be(failure);
            }

            [Fact]
            public async Task WhenSignatureCannotBeParsed_ReturnsFailureResult() {
                var failure = new SignatureVerificationException("Cannot parse signature.");
                A.CallTo(() => _signatureParser.Parse(A<HttpRequestMessage>.That.Matches(r => r.RequestUri == _expectedUri)))
                    .Throws(failure);

                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().SignatureVerificationException.Should().Be(failure);
            }

            [Fact]
            public async Task WhenClientDoesNotExist_ReturnsFailureResult() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(A<HttpRequestMessage>.That.Matches(r => r.RequestUri == _expectedUri)))
                    .Returns(signature);

                var failure = new SignatureVerificationException("Don't know that client.");
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Throws(failure);

                var actual = await _sut.VerifySignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
                actual.As<RequestSignatureVerificationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureVerificationResultFailure>().SignatureVerificationException.Should().Be(failure);
            }
        }
    }
}