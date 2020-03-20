using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.SigningString;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class SignatureCreatorTests {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;
        private readonly ISigningSettingsSanitizer _signingSettingsSanitizer;
        private readonly INonceGenerator _nonceGenerator;
        private readonly ILogger<SignatureCreator> _logger;
        private readonly SignatureCreator _sut;

        public SignatureCreatorTests() {
            FakeFactory.Create(out _base64Converter, out _signingStringComposer, out _signingSettingsSanitizer, out _nonceGenerator, out _logger);
            _sut = new SignatureCreator(_signingSettingsSanitizer, _signingStringComposer, _base64Converter, _nonceGenerator, _logger);
        }

        public class CreateSignature : SignatureCreatorTests {
            private readonly HttpRequestMessage _httpRequestMessage;
            private readonly SigningSettings _settings;
            private readonly DateTimeOffset _timeOfSigning;
            private string _nonce;

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
                    }
                };
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.Zero);
                A.CallTo(() => _settings.SignatureAlgorithm.Name).Returns("Custom");
                _nonce = "abc123";
                A.CallTo(() => _nonceGenerator.GenerateNonce()).Returns(_nonce);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateSignature(null, _settings, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateSignature(_httpRequestMessage, null, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsValidationException() {
                _settings.KeyId = KeyId.Empty; // Make invalid
                Action act = () => _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void SanitizesHeaderNamesToInclude() {
                _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);

                A.CallTo(() => _signingSettingsSanitizer.SanitizeHeaderNamesToInclude(_settings, _httpRequestMessage))
                    .MustHaveHappened();
            }
            
            [Fact]
            public void CalculatesSignatureForExpectedRequestForSigning() {
                var composedString = "{the composed string}";
                HttpRequestForSigning interceptedRequest = null;
                A.CallTo(() => _signingStringComposer.Compose(A<HttpRequestForSigning>._, _settings.Headers, _timeOfSigning, _settings.Expires, _nonce))
                    .Invokes(call => interceptedRequest = call.GetArgument<HttpRequestForSigning>(0))
                    .Returns(composedString);

                _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);

                interceptedRequest.Should().BeEquivalentTo(new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1"),
                    Headers = new HeaderDictionary(new Dictionary<string, StringValues> {{"H1", "v1"}}),
                    SignatureAlgorithmName = "Custom"
                });
            }

            [Fact]
            public void ReturnsSignatureWithCalculatedSignatureString() {
                var composedString = "{the composed string}";
                A.CallTo(() => _signingStringComposer.Compose(A<HttpRequestForSigning>._, _settings.Headers, _timeOfSigning, _settings.Expires, _nonce))
                    .Returns(composedString);

                var signatureHash = new byte[] {0x03, 0x04};
                A.CallTo(() => _settings.SignatureAlgorithm.ComputeHash(composedString))
                    .Returns(signatureHash);

                var signatureString = "xyz=";
                A.CallTo(() => _base64Converter.ToBase64(signatureHash))
                    .Returns(signatureString);

                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);

                actual.String.Should().Be(signatureString);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedKeyId() {
                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);
                actual.KeyId.Should().Be(_settings.KeyId);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedAlgorithm() {
                A.CallTo(() => _settings.SignatureAlgorithm.Name).Returns("RSA");
                A.CallTo(() => _settings.SignatureAlgorithm.HashAlgorithm).Returns(HashAlgorithmName.SHA512);

                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);

                actual.Algorithm.Should().Be("rsa-sha512");
            }

            [Fact]
            public void ReturnsSignatureWithExpectedCreatedValue() {
                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);
                actual.Created.Should().Be(_timeOfSigning);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedExpiresValue() {
                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);
                actual.Expires.Should().Be(_timeOfSigning.Add(_settings.Expires));
            }

            [Fact]
            public void ReturnsSignatureWithExpectedNonceValue() {
                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);
                actual.Nonce.Should().Be(_nonce);
            }
            
            [Fact]
            public void ReturnsSignatureWithExpectedHeaders() {
                var actual = _sut.CreateSignature(_httpRequestMessage, _settings, _timeOfSigning);
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