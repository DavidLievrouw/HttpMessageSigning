using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FakeItEasy.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class MatchingSignatureStringVerificationTaskTests {
        private readonly IBase64Converter _base64Converter;
        private readonly ILogger<MatchingSignatureStringVerificationTask> _logger;
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly ISigningStringCompositionRequestFactory _stringCompositionRequestFactory;
        private readonly MatchingSignatureStringVerificationTask _sut;

        public MatchingSignatureStringVerificationTaskTests() {
            FakeFactory.Create(out _signingStringComposer, out _base64Converter, out _stringCompositionRequestFactory, out _logger);
            _sut = new MatchingSignatureStringVerificationTask(_signingStringComposer, _base64Converter, _stringCompositionRequestFactory, _logger);
        }

        public class Verify : MatchingSignatureStringVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForVerification, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForVerification _signedRequest;
            private readonly string _composedSignatureString;
            private readonly CustomSignatureAlgorithm _signatureAlgorithm;
            private readonly DateTimeOffset _now;
            private readonly Func<IReturnValueArgumentValidationConfiguration<string>> _composeCall;

            public Verify() {
                _now = new DateTimeOffset(2020, 2, 24, 10, 20, 14, TimeSpan.FromHours(0));
                
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForVerification) TestModels.RequestForVerification.Clone();
                _signatureAlgorithm = new CustomSignatureAlgorithm("TEST");
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, _signatureAlgorithm, TestModels.Client.NonceLifetime, TestModels.Client.ClockSkew, TestModels.Client.Claims);
                _method = (request, signature, client) => _sut.Verify(request, signature, client);

                _composedSignatureString = "abc123";
                _composeCall = () => A.CallTo(() => _signingStringComposer.Compose(A<SigningStringCompositionRequest>._));
                _composeCall().Returns(_composedSignatureString);
                _signature.String = _base64Converter.ToBase64(_client.SignatureAlgorithm.ComputeHash(_composedSignatureString));
            }

            [Fact]
            public async Task WhenEverythingIsSpecified_CallsSigningStringComposerWithCreatedCompositionRequest() {
                var compositionRequest = new SigningStringCompositionRequest {
                    Request = _signedRequest.ToHttpRequestForSignatureString(),
                    Expires = _signature.Expires.Value - _signature.Created.Value,
                    HeadersToInclude = _signature.Headers,
                    RequestTargetEscaping = RequestTargetEscaping.RFC3986, // ToDo #13
                    TimeOfComposing = _now
                };
                A.CallTo(() => _stringCompositionRequestFactory.CreateForVerification(_signedRequest, _client, _signature))
                    .Returns(compositionRequest);
                
                SigningStringCompositionRequest interceptedRequest = null;

                _composeCall().Invokes(call => interceptedRequest = call.GetArgument<SigningStringCompositionRequest>(0))
                    .Returns(_composedSignatureString);

                await _method(_signedRequest, _signature, _client);

                interceptedRequest.Should().Be(compositionRequest);
            }

            [Fact]
            public async Task WhenSignatureStringIsNotAValidBase64String_ReturnsSignatureVerificationFailure() {
                var formatEx = new FormatException("Invalid base64 string.");
                A.CallTo(() => _base64Converter.FromBase64(_signature.String))
                    .Throws(formatEx);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>();
                actual.As<SignatureVerificationFailure>().Code.Should().Be("INVALID_SIGNATURE_STRING");
                actual.As<SignatureVerificationFailure>().Exception.Should().Be(formatEx);
            }

            [Fact]
            public async Task WhenSignatureStringCannotBeVerified_ReturnsSignatureVerificationFailure() {
                var receivedSignature = new byte[] {0x01, 0x02, 0x03};
                A.CallTo(() => _base64Converter.FromBase64(_signature.String))
                    .Returns(receivedSignature);

                _signatureAlgorithm.SetVerificationResult(false);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_SIGNATURE_STRING");
            }

            [Fact]
            public async Task WhenSignatureStringIsVerified_ReturnsNull() {
                var receivedSignature = new byte[] {0x01, 0x02, 0x03};
                A.CallTo(() => _base64Converter.FromBase64(_signature.String))
                    .Returns(receivedSignature);
                
                _signatureAlgorithm.SetVerificationResult(true);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}