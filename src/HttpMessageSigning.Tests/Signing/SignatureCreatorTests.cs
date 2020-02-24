using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.SigningString;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class SignatureCreatorTests {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IKeyedHashAlgorithmFactory _keyedHashAlgorithmFactory;
        private readonly IBase64Converter _base64Converter;
        private readonly ISystemClock _systemClock;
        private readonly IHttpMessageSigningLogger<SignatureCreator> _logger;
        private readonly SignatureCreator _sut;

        public SignatureCreatorTests() {
            FakeFactory.Create(out _base64Converter, out _keyedHashAlgorithmFactory, out _signingStringComposer, out _systemClock, out _logger);
            _sut = new SignatureCreator(_signingStringComposer, _keyedHashAlgorithmFactory, _base64Converter, _systemClock, _logger);
        }

        public class CreateSignature : SignatureCreatorTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;
            private readonly DateTimeOffset _now;

            public CreateSignature() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _settings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    KeyId = new KeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123"),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    }
                };
                _now = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.Zero);
                A.CallTo(() => _systemClock.UtcNow).Returns(_now);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateSignature(null, _settings);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateSignature(_httpRequest, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsHttpMessageSigningValidationException() {
                _settings.KeyId = null; // Make invalid
                Action act = () => _sut.CreateSignature(_httpRequest, _settings);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }

            [Fact]
            public void ReturnsSignatureWithCalculatedSignatureString() {
                var composedString = "{the composed string}";
                A.CallTo(() => _signingStringComposer.Compose(_httpRequest, _settings, _now))
                    .Returns(composedString);

                var signingKey = new byte[] {0x01, 0x02};
                A.CallTo(() => _base64Converter.FromBase64(_settings.KeyId.Key))
                    .Returns(signingKey);

                var hashAlgorithm = A.Fake<IKeyedHashAlgorithm>();
                A.CallTo(() => _keyedHashAlgorithmFactory.Create(_settings.KeyId.SignatureAlgorithm, _settings.KeyId.HashAlgorithm, signingKey))
                    .Returns(hashAlgorithm);

                var signatureHash = new byte[] {0x03, 0x04};
                A.CallTo(() => hashAlgorithm.ComputeHash(composedString))
                    .Returns(signatureHash);

                var signatureString = "xyz=";
                A.CallTo(() => _base64Converter.ToBase64(signatureHash))
                    .Returns(signatureString);

                var actual = _sut.CreateSignature(_httpRequest, _settings);

                actual.String.Should().Be(signatureString);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedKeyId() {
                var actual = _sut.CreateSignature(_httpRequest, _settings);
                actual.KeyId.Should().Be(_settings.KeyId);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedAlgorithm() {
                var actual = _sut.CreateSignature(_httpRequest, _settings);
                actual.SignatureAlgorithm.Should().Be(SignatureAlgorithm.HMAC);
                actual.HashAlgorithm.Should().Be(HashAlgorithm.SHA256);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedCreatedValue() {
                var actual = _sut.CreateSignature(_httpRequest, _settings);
                actual.Created.Should().Be(_now);
            }

            [Fact]
            public void ReturnsSignatureWithExpectedExpiresValue() {
                var actual = _sut.CreateSignature(_httpRequest, _settings);
                actual.Expires.Should().Be(_now.Add(_settings.Expires));
            }

            [Fact]
            public void ReturnsSignatureWithExpectedHeaders() {
                var actual = _sut.CreateSignature(_httpRequest, _settings);
                actual.Headers.Should().BeEquivalentTo(
                    new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    },
                    options => options.WithStrictOrdering());
            }
        }
    }
}