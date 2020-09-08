using System;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class SigningStringCompositionRequestFactoryTests {
        private readonly INonceGenerator _nonceGenerator;
        private readonly SigningStringCompositionRequestFactory _sut;

        public SigningStringCompositionRequestFactoryTests() {
            FakeFactory.Create(out _nonceGenerator);
            _sut = new SigningStringCompositionRequestFactory(_nonceGenerator);
        }

        public class CreateForSigning : SigningStringCompositionRequestFactoryTests {
            private readonly HttpRequestForSigning _request;
            private readonly SigningSettings _settings;
            private readonly DateTimeOffset? _timeOfComposingUtc;
            private readonly TimeSpan _expires;

            public CreateForSigning() {
                _request = new HttpRequestForSigning();
                _settings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    KeyId = new KeyId("client1"),
                    SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    },
                    DigestHashAlgorithm = HashAlgorithmName.SHA256,
                    EnableNonce = true,
                    RequestTargetEscaping = RequestTargetEscaping.RFC2396,
                    AutomaticallyAddRecommendedHeaders = true
                };
                _timeOfComposingUtc = new DateTimeOffset(2020, 9, 8, 13, 21, 14, TimeSpan.Zero);
                _expires = TimeSpan.FromMinutes(3);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForSigning(null, _settings, _timeOfComposingUtc, _expires);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSigningSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForSigning(_request, null, _timeOfComposingUtc, _expires);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CreatesRequestWithExpectedPropertyValues() {
                var actual = _sut.CreateForSigning(_request, _settings, _timeOfComposingUtc, _expires);

                actual.Request.Should().BeEquivalentTo(_request.ToHttpRequestForSignatureString());
                actual.RequestTargetEscaping.Should().Be(_settings.RequestTargetEscaping);
                actual.HeadersToInclude.Should().BeEquivalentTo(_settings.Headers, opts => opts.WithStrictOrdering());
                actual.TimeOfComposing.Should().Be(_timeOfComposingUtc);
                actual.Expires.Should().Be(_expires);
            }

            [Fact]
            public void WhenNonceIsEnabled_GeneratesNonce() {
                var generatedNonce = "abc123";
                A.CallTo(() => _nonceGenerator.GenerateNonce())
                    .Returns(generatedNonce);

                _settings.EnableNonce = true;

                var actual = _sut.CreateForSigning(_request, _settings, _timeOfComposingUtc, _expires);

                actual.Nonce.Should().Be(generatedNonce);
            }

            [Fact]
            public void WhenNonceIsDisabled_SetsNonceToNull() {
                var generatedNonce = "abc123";
                A.CallTo(() => _nonceGenerator.GenerateNonce())
                    .Returns(generatedNonce);

                _settings.EnableNonce = false;

                var actual = _sut.CreateForSigning(_request, _settings, _timeOfComposingUtc, _expires);

                actual.Nonce.Should().BeNull();
            }

            [Fact]
            public void GivenNullTimeOfComposingAndExpiration_SetsPropertiesToNull() {
                var actual = _sut.CreateForSigning(_request, _settings, timeOfComposing: null, expires: null);

                actual.TimeOfComposing.Should().BeNull();
                actual.Expires.Should().BeNull();
            }
        }
    }
}