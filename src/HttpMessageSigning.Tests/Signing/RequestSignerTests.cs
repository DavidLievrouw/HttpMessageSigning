using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Dalion.HttpMessageSigning.Logging;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerTests {
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureCreator _signatureCreator;
        private readonly SigningSettings _signingSettings;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;
        private readonly RequestSigner _sut;

        public RequestSignerTests() {
            FakeFactory.Create(out _signatureCreator, out _authorizationHeaderParamCreator, out _logger);
            _signingSettings = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                KeyId = new KeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123"),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Expires,
                    new HeaderName("dalion_app_id")
                }
            };
            _sut = new RequestSigner(_signatureCreator, _authorizationHeaderParamCreator, _signingSettings, _logger);
        }

        public class Sign : RequestSignerTests {
            private readonly HttpRequestMessage _httpRequest;

            public Sign() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Sign(null);
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Fact]
            public void GivenInvalidSettings_ThrowsHttpMessageSigningValidationException() {
                _signingSettings.KeyId = null; // Make invalid
                Action act = () => _sut.Sign(_httpRequest);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void SetsExpectedAuthorizationHeaderInRequest() {
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Signature", "signature=abc123="));
            }

            [Fact]
            public void OverwritesAuthorizationHeaderValueInRequest() {
                _httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Custom", "john.doe");

                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Signature", "signature=abc123="));
            }
        }
    }
}