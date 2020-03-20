using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.SigningString;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class MatchingSignatureStringVerificationTaskTests {
        private readonly IBase64Converter _base64Converter;
        private readonly ILogger<MatchingSignatureStringVerificationTask> _logger;
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly MatchingSignatureStringVerificationTask _sut;

        public MatchingSignatureStringVerificationTaskTests() {
            FakeFactory.Create(out _signingStringComposer, out _base64Converter, out _logger);
            _sut = new MatchingSignatureStringVerificationTask(_signingStringComposer, _base64Converter, _logger);
        }

        public class Verify : MatchingSignatureStringVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<Exception>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;
            private readonly string _composedSignatureString;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForSigning) TestModels.Request.Clone();
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, A.Fake<ISignatureAlgorithm>(), TimeSpan.FromMinutes(1));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);

                _composedSignatureString = "abc123";
                A.CallTo(() => _signingStringComposer.Compose(A<HttpRequestForSigning>._, A<HeaderName[]>._, A<DateTimeOffset>._, A<TimeSpan>._, A<string>._))
                    .Returns(_composedSignatureString);
                _signature.String = _base64Converter.ToBase64(_client.SignatureAlgorithm.ComputeHash(_composedSignatureString));
            }

            [Fact]
            public async Task WhenSignatureDoesNotSpecifyACreationTime_ReturnsSignatureVerificationException() {
                _signature.Created = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenSignatureDoesNotSpecifyAExpirationTime_ReturnsSignatureVerificationException() {
                _signature.Expires = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task CallsSigningStringComposerWithExpectedParameters() {
                HttpRequestForSigning interceptedRequest = null;
                HeaderName[] interceptedHeaderNames = null;
                DateTimeOffset? interceptedCreated = null;
                TimeSpan? interceptedExpires = null;

                A.CallTo(() => _signingStringComposer.Compose(A<HttpRequestForSigning>._, A<HeaderName[]>._, A<DateTimeOffset>._, A<TimeSpan>._, A<string>._))
                    .Invokes(call => {
                        interceptedRequest = call.GetArgument<HttpRequestForSigning>(0);
                        interceptedHeaderNames = call.GetArgument<HeaderName[]>(1);
                        interceptedCreated = call.GetArgument<DateTimeOffset>(2);
                        interceptedExpires = call.GetArgument<TimeSpan>(3);
                    })
                    .Returns(_composedSignatureString);

                await _method(_signedRequest, _signature, _client);

                interceptedCreated.Should().Be(_signature.Created);
                interceptedExpires.Should().Be(_signature.Expires.Value - _signature.Created.Value);
                interceptedRequest.Should().Be(_signedRequest);
                interceptedHeaderNames.Should().Equal(_signature.Headers);
            }

            [Fact]
            public async Task WhenSignatureStringCannotBeVerified_ReturnsSignatureVerificationException() {
                var receivedSignature = new byte[] {0x01, 0x02, 0x03};
                A.CallTo(() => _base64Converter.FromBase64(_signature.String))
                    .Returns(receivedSignature);

                A.CallTo(() => _client.SignatureAlgorithm.VerifySignature(_composedSignatureString, A<byte[]>.That.IsSameSequenceAs(receivedSignature)))
                    .Returns(false);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenSignatureStringIsVerified_ReturnsNull() {
                var receivedSignature = new byte[] {0x01, 0x02, 0x03};
                A.CallTo(() => _base64Converter.FromBase64(_signature.String))
                    .Returns(receivedSignature);

                A.CallTo(() => _client.SignatureAlgorithm.VerifySignature(_composedSignatureString, A<byte[]>.That.IsSameSequenceAs(receivedSignature)))
                    .Returns(true);
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}