using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class RequestSignatureValidatorTests {
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly IClientStore _clientStore;
        private readonly ISignatureParser _signatureParser;
        private readonly ISignatureValidator _signatureValidator;
        private readonly RequestSignatureValidator _sut;

        public RequestSignatureValidatorTests() {
            FakeFactory.Create(out _signatureParser, out _clientStore, out _signatureValidator, out _claimsPrincipalFactory);
            _sut = new RequestSignatureValidator(_signatureParser, _clientStore, _signatureValidator, _claimsPrincipalFactory);
        }

        public class ValidateSignature : RequestSignatureValidatorTests {
            private readonly DefaultHttpRequest _request;

            public ValidateSignature() {
                _request = new DefaultHttpRequest(new DefaultHttpContext());
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.ValidateSignature(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task ValidatesSignatureOfClient_ThatMatchesTheKeyIdFromTheRequest() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_request))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithm.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                await _sut.ValidateSignature(_request);

                A.CallTo(() => _signatureValidator.ValidateSignature(signature, client))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenValidationSucceeds_ReturnsSuccessResultWithClaimsPrincipal() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_request))
                    .Returns(signature);

                var client = new Client(signature.KeyId, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithm.SHA256));
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Returns(client);

                var principal = new ClaimsPrincipal();
                A.CallTo(() => _claimsPrincipalFactory.CreateForClient(client))
                    .Returns(principal);

                var actual = await _sut.ValidateSignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureValidationResultSuccess>();
                actual.As<RequestSignatureValidationResultSuccess>().IsSuccess.Should().BeTrue();
                actual.As<RequestSignatureValidationResultSuccess>().ValidatedPrincipal.Should().Be(principal);
            }

            [Fact]
            public async Task WhenValidationFails_ReturnsFailureResult() {
                var failure = new SignatureValidationException("Invalid signature.");
                A.CallTo(() => _signatureValidator.ValidateSignature(A<Signature>._, A<Client>._))
                    .Throws(failure);

                var actual = await _sut.ValidateSignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureValidationResultFailure>();
                actual.As<RequestSignatureValidationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureValidationResultFailure>().SignatureValidationException.Should().Be(failure);
            }

            [Fact]
            public async Task WhenSignatureCannotBeParsed_ReturnsFailureResult() {
                var failure = new SignatureValidationException("Cannot parse signature.");
                A.CallTo(() => _signatureParser.Parse(_request))
                    .Throws(failure);

                var actual = await _sut.ValidateSignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureValidationResultFailure>();
                actual.As<RequestSignatureValidationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureValidationResultFailure>().SignatureValidationException.Should().Be(failure);
            }

            [Fact]
            public async Task WhenClientDoesNotExist_ReturnsFailureResult() {
                var signature = new Signature {KeyId = new KeyId("app001")};
                A.CallTo(() => _signatureParser.Parse(_request))
                    .Returns(signature);

                var failure = new SignatureValidationException("Don't know that client.");
                A.CallTo(() => _clientStore.Get(signature.KeyId))
                    .Throws(failure);

                var actual = await _sut.ValidateSignature(_request);

                actual.Should().BeAssignableTo<RequestSignatureValidationResultFailure>();
                actual.As<RequestSignatureValidationResultFailure>().IsSuccess.Should().BeFalse();
                actual.As<RequestSignatureValidationResultFailure>().SignatureValidationException.Should().Be(failure);
            }
        }
    }
}