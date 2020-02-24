using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerTests {
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureCreator _signatureCreator;
        private readonly RequestSigner _sut;

        public RequestSignerTests() {
            FakeFactory.Create(out _signatureCreator, out _authorizationHeaderParamCreator);
            _sut = new RequestSigner(_signatureCreator, _authorizationHeaderParamCreator);
        }

        public class Sign : RequestSignerTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;

            public Sign() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _settings = new SigningSettings {
                    Algorithm = Algorithm.hmac_sha256,
                    Expires = TimeSpan.FromMinutes(5),
                    KeyId = new KeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123"),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    }
                };
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Sign(null, _settings);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.Sign(_httpRequest, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsHttpMessageSigningValidationException() {
                _settings.KeyId = null; // Make invalid
                Action act = () => _sut.Sign(_httpRequest, _settings);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void SetsExpectedAuthorizationHeaderInRequest() {
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _settings))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                _sut.Sign(_httpRequest, _settings);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Signature", "signature=abc123="));
            }

            [Fact]
            public void OverwritesAuthorizationHeaderValueInRequest() {
                _httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Custom", "john.doe");

                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _settings))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                _sut.Sign(_httpRequest, _settings);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Signature", "signature=abc123="));
            }
        }
    }
}