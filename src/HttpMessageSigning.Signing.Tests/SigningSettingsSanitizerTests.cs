using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class SigningSettingsSanitizerTests {
        private readonly SigningSettingsSanitizer _sut;

        public SigningSettingsSanitizerTests() {
            _sut = new SigningSettingsSanitizer();
        }

        public class SanitizeHeaderNamesToInclude : SigningSettingsSanitizerTests {
            private readonly SigningSettings _settings;
            private readonly HttpRequestMessage _httpRequest;

            public SanitizeHeaderNamesToInclude() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
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
                    DigestHashAlgorithm = HashAlgorithmName.SHA256
                };
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.SanitizeHeaderNamesToInclude(null, _httpRequest);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.SanitizeHeaderNamesToInclude(_settings, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenHeadersIsNull_AndFeatureIsDisabled_KeepsNullHeaders() {
                _settings.Headers = null;
                _settings.AutomaticallyAddRecommendedHeaders = false;

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().BeNull();
            }
            
            [Fact]
            public void WhenHeadersIsEmpty_AndFeatureIsDisabled_KeepsEmptyHeaders() {
                _settings.Headers = Array.Empty<HeaderName>();
                _settings.AutomaticallyAddRecommendedHeaders = false;

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().BeEmpty();
            }

            [Fact]
            public void WhenHeadersIsNull_AndFeatureIsEnabled_DoesNotThrow() {
                _settings.Headers = null;
                _settings.AutomaticallyAddRecommendedHeaders = true;

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().NotBeNull();
                _settings.Headers.Should().NotBeEmpty();
            }
            
            [Fact]
            public void WhenFeatureIsDisabled_DoesNotTakeAction() {
                _settings.AutomaticallyAddRecommendedHeaders = false;

                _settings.Headers = new[] {HeaderName.PredefinedHeaderNames.Date};

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().BeEquivalentTo(new[] {HeaderName.PredefinedHeaderNames.Date});
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndHeadersDoesNotContainRequestTarget_AddsRequestTargetToHeaders() {
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().Contain(HeaderName.PredefinedHeaderNames.RequestTarget);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndAlgorithmIsRSAOrHMACOrECDSA_AndHeadersDoesNotContainDate_AddsDateToHeaders() {
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().Contain(HeaderName.PredefinedHeaderNames.Date);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndAlgorithmIsNotRSAOrHMACOrECDSA_AndHeadersDoesNotContainDate_DoesNotAddDateToHeaders() {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm("SomethingElse");
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().NotContain(HeaderName.PredefinedHeaderNames.Date);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndAlgorithmIsNotRSAOrHMACOrECDSA_AndHeadersDoesNotContainCreated_AddsCreatedToHeaders() {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm("SomethingElse");
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().Contain(HeaderName.PredefinedHeaderNames.Created);
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public void WhenFeatureIsEnabled_AndAlgorithmIsRSAOrHMACOrECDSA_AndHeadersDoesNotContainCreated_DoesNotAddCreatedToHeaders(string algorithmName) {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm(algorithmName);
                _settings.Headers = new[] {HeaderName.PredefinedHeaderNames.Date};

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().NotContain(HeaderName.PredefinedHeaderNames.Created);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndAlgorithmIsNotRSAOrHMACOrECDSA_AndHeadersDoesNotContainExpires_AddsExpiresToHeaders() {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm("SomethingElse");
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().Contain(HeaderName.PredefinedHeaderNames.Expires);
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public void WhenFeatureIsEnabled_AndAlgorithmIsRSAOrHMACOrECDSA_AndHeadersDoesNotContainExpires_DoesNotAddExpiresToHeaders(string algorithmName) {
                _settings.SignatureAlgorithm = new CustomSignatureAlgorithm(algorithmName);
                _settings.Headers = new[] {HeaderName.PredefinedHeaderNames.Date};

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().NotContain(HeaderName.PredefinedHeaderNames.Expires);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndHeadersDoesNotContainDigest_AndDigestIsOff_DoesNotAddDigestHeader() {
                _settings.DigestHashAlgorithm = default;
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().NotContain(_ => _.Value == HeaderName.PredefinedHeaderNames.Digest);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndHeadersDoesNotContainDigest_AndDigestIsOn_AddsDigestHeader() {
                _settings.DigestHashAlgorithm = HashAlgorithmName.SHA384;
                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().Contain(_ => _.Value == HeaderName.PredefinedHeaderNames.Digest);
            }

            [Fact]
            public void WhenFeatureIsEnabled_AndHeadersContainsDigest_AndDigestIsOn_DoesNotAddDigestHeaderAgain() {
                _settings.Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Digest,
                    HeaderName.PredefinedHeaderNames.Expires,
                    new HeaderName("dalion_app_id")
                };
                _settings.DigestHashAlgorithm = HashAlgorithmName.SHA384;

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().Contain(_ => _.Value == HeaderName.PredefinedHeaderNames.Digest);
                _settings.Headers.Count(_ => _.Value == HeaderName.PredefinedHeaderNames.Digest).Should().Be(1);
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("TRACE")]
            [InlineData("HEAD")]
            [InlineData("DELETE")]
            public void WhenFeatureIsEnabled_AndHeadersDoesNotContainDigest_AndDigestIsOn_ButMethodDoesNotHaveBody_DoesNotAddDigestHeader(string method) {
                _settings.DigestHashAlgorithm = HashAlgorithmName.SHA384;
                _httpRequest.Method = new HttpMethod(method);

                _settings.Headers = Array.Empty<HeaderName>();

                _sut.SanitizeHeaderNamesToInclude(_settings, _httpRequest);

                _settings.Headers.Should().NotContain(_ => _.Value == HeaderName.PredefinedHeaderNames.Digest);
            }
        }
    }
}