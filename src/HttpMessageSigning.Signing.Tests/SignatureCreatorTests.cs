using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class SignatureCreatorTests {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;
        private readonly ISigningStringCompositionRequestFactory _stringCompositionRequestFactory;
        private readonly ILogger<SignatureCreator> _logger;
        private readonly SignatureCreator _sut;

        public SignatureCreatorTests() {
            FakeFactory.Create(out _base64Converter, out _signingStringComposer, out _stringCompositionRequestFactory, out _logger);
            _sut = new SignatureCreator(_signingStringComposer, _base64Converter, _stringCompositionRequestFactory, _logger);
        }

        public class CreateSignature : SignatureCreatorTests {
            private readonly HttpRequestMessage _httpRequestMessage;
            private readonly SigningSettings _settings;
            private readonly DateTimeOffset _timeOfSigning;
            private readonly string _nonce;
            private readonly TimeSpan _expires;

            public CreateSignature() {
                _httpRequestMessage = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1"),
                    Headers = {{"H1", "v1"}}
                };
                _settings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    KeyId = new KeyId("client1"),
                    SignatureAlgorithm = A.Fake<ISignatureAlgorithm>(),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        new HeaderName("dalion_app_id")
                    },
                    UseDeprecatedAlgorithmParameter = false,
                    AuthorizationScheme = "TestScheme",
                    EnableNonce = true,
                    DigestHashAlgorithm = HashAlgorithmName.SHA384,
                    RequestTargetEscaping = RequestTargetEscaping.Unescaped
                };
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.Zero);
                _expires = TimeSpan.FromMinutes(10);
                A.CallTo(() => _settings.SignatureAlgorithm.Name).Returns("Custom");
                _nonce = "abc123";
            }

            [Fact]
            public async Task GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.CreateSignature(null, _settings, _timeOfSigning, _expires);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }

            [Fact]
            public async Task GivenNullSettings_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.CreateSignature(_httpRequestMessage, null, _timeOfSigning, _expires);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }
            
            [Fact]
            public async Task CalculatesSignatureForExpectedRequest() {
                var compositionRequest = new SigningStringCompositionRequest();
                HttpRequestForSigning interceptedRequest = null;
                SigningSettings interceptedSigningSettings = null;
                DateTimeOffset? interceptedTimeOfSigning = null;
                TimeSpan? interceptedExpires = null;
                A.CallTo(() => _stringCompositionRequestFactory.CreateForSigning(A<HttpRequestForSigning>._, A<SigningSettings>._, A<DateTimeOffset?>._, A<TimeSpan?>._))
                    .Invokes(call => {
                        interceptedRequest = call.GetArgument<HttpRequestForSigning>(0);
                        interceptedSigningSettings = call.GetArgument<SigningSettings>(1);
                        interceptedTimeOfSigning = call.GetArgument<DateTimeOffset?>(2);
                        interceptedExpires = call.GetArgument<TimeSpan?>(3);
                    })
                    .Returns(compositionRequest);
                
                var composedString = "{the composed string}";
                A.CallTo(() => _signingStringComposer.Compose(A<SigningStringCompositionRequest>._))
                    .Returns(composedString);

                await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);

                interceptedRequest.Should().BeEquivalentTo(new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = "http://dalion.eu/api/resource/id1".ToUri(),
                    Headers = new HeaderDictionary(new Dictionary<string, StringValues> {{"H1", "v1"}})
                });
                interceptedSigningSettings.Should().Be(_settings);
                interceptedTimeOfSigning.Should().Be(_timeOfSigning);
                interceptedExpires.Should().Be(_expires);
            }

            [Fact]
            public async Task ReturnsSignatureWithCalculatedSignatureString() {
                var composedString = "{the composed string}";
                A.CallTo(() => _signingStringComposer.Compose(A<SigningStringCompositionRequest>._))
                    .Returns(composedString);

                var signatureHash = new byte[] {0x03, 0x04};
                A.CallTo(() => _settings.SignatureAlgorithm.ComputeHash(composedString))
                    .Returns(signatureHash);

                var signatureString = "xyz=";
                A.CallTo(() => _base64Converter.ToBase64(signatureHash))
                    .Returns(signatureString);

                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);

                actual.String.Should().Be(signatureString);
            }

            [Fact]
            public async Task ReturnsSignatureWithExpectedKeyId() {
                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);
                actual.KeyId.Should().Be(_settings.KeyId);
            }

            [Fact]
            public async Task WhenDeprecatedAlgorithmParameterIsDisabled_ReturnsSignatureWithExpectedAlgorithm() {
                _settings.UseDeprecatedAlgorithmParameter = false;
                A.CallTo(() => _settings.SignatureAlgorithm.Name).Returns("RSA");
                A.CallTo(() => _settings.SignatureAlgorithm.HashAlgorithm).Returns(HashAlgorithmName.SHA512);

                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);

                actual.Algorithm.Should().Be("hs2019");
            }
            
            [Fact]
            public async Task WhenDeprecatedAlgorithmParameterIsEnabled_ReturnsSignatureWithDeprecatedAlgorithm() {
                _settings.UseDeprecatedAlgorithmParameter = true;
                A.CallTo(() => _settings.SignatureAlgorithm.Name).Returns("RSA");
                A.CallTo(() => _settings.SignatureAlgorithm.HashAlgorithm).Returns(HashAlgorithmName.SHA512);

                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);

                actual.Algorithm.Should().Be("rsa-sha512");
            }

            [Fact]
            public async Task ReturnsSignatureWithExpectedCreatedValue() {
                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);
                actual.Created.Should().Be(_timeOfSigning);
            }

            [Fact]
            public async Task ReturnsSignatureWithExpectedExpiresValue_NotFromTheSettings() {
                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);
                actual.Expires.Should().Be(_timeOfSigning.Add(_expires));
            }

            [Fact]
            public async Task ReturnsSignatureWithExpectedNonceValue() {
                var compositionRequest = new SigningStringCompositionRequest {
                    Nonce = _nonce
                };
                A.CallTo(() => _stringCompositionRequestFactory.CreateForSigning(A<HttpRequestForSigning>._, A<SigningSettings>._, A<DateTimeOffset?>._, A<TimeSpan?>._))
                    .Returns(compositionRequest);
                
                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);
                actual.Nonce.Should().Be(_nonce);
            }
            
            [Fact]
            public async Task InvokesOnSigningStringComposedEvent() {
                string interceptedSigningString = null;
                _settings.Events.OnSigningStringComposed = (HttpRequestMessage requestToSign, ref string signingString) => {
                    interceptedSigningString = signingString;
                    return Task.CompletedTask;
                };
                
                var composedString = "{the composed string}";
                A.CallTo(() => _signingStringComposer.Compose(A<SigningStringCompositionRequest>._))
                    .Returns(composedString);
                
                await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);

                interceptedSigningString.Should().Be(composedString);
            }
            
            [Fact]
            public async Task GivenNullOnSigningStringComposedEvent_DoesNotThrow() {
                _settings.Events.OnSigningStringComposed = null;
                
                Func<Task> act = () => _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);
                
                await act.Should().NotThrowAsync();
            }
            
            [Fact]
            public async Task ReturnsSignatureWithExpectedHeaders() {
                var compositionRequest = new SigningStringCompositionRequest {
                    HeadersToInclude = _settings.Headers
                };
                A.CallTo(() => _stringCompositionRequestFactory.CreateForSigning(A<HttpRequestForSigning>._, A<SigningSettings>._, A<DateTimeOffset?>._, A<TimeSpan?>._))
                    .Returns(compositionRequest);
                
                var actual = await _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning, _expires);
                actual.Headers.Should().BeEquivalentTo(
                    new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        new HeaderName("dalion_app_id")
                    },
                    options => options.WithStrictOrdering());
            }
        }
    }
}