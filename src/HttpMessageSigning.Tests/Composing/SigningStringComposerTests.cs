using System;
using System.Linq;
using System.Net.Http;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Composing {
    public class SigningStringComposerTests {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;
        private readonly SigningStringComposer _sut;

        public SigningStringComposerTests() {
            FakeFactory.Create(out _headerAppenderFactory);
            _sut = new SigningStringComposer(_headerAppenderFactory);
        }

        public class Compose : SigningStringComposerTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;
            private readonly IHeaderAppender _headerAppender;

            public Compose() {
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
                        new HeaderName("reco_app_id")
                    }
                };

                FakeFactory.Create(out _headerAppender);
                A.CallTo(() => _headerAppenderFactory.Create(_httpRequest, _settings))
                    .Returns(_headerAppender);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(null, _settings);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(_httpRequest, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsHttpMessageSigningValidationException() {
                _settings.KeyId = null; // Make invalid
                Action act = () => _sut.Compose(_httpRequest, _settings);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }

            [Fact]
            public void WhenHeadersDoesNotContainRequestTarget_PrependsRequestTargetToHeaders() {
                SigningSettings interceptedSettings = null;
                A.CallTo(() => _headerAppenderFactory.Create(_httpRequest, _settings))
                    .Invokes(call => interceptedSettings = call.GetArgument<SigningSettings>(1))
                    .Returns(_headerAppender);

                _settings.Headers = Array.Empty<HeaderName>();
                _sut.Compose(_httpRequest, _settings);

                interceptedSettings.Headers.ElementAt(0).Should().Be(HeaderName.PredefinedHeaderNames.RequestTarget);
            }

            [Fact]
            public void WhenHeadersDoesNotContainDate_PrependsDateToHeaders_ButAfterRequestTargetHeader() {
                SigningSettings interceptedSettings = null;
                A.CallTo(() => _headerAppenderFactory.Create(_httpRequest, _settings))
                    .Invokes(call => interceptedSettings = call.GetArgument<SigningSettings>(1))
                    .Returns(_headerAppender);

                _settings.Headers = Array.Empty<HeaderName>();
                _sut.Compose(_httpRequest, _settings);

                interceptedSettings.Headers.ElementAt(0).Should().Be(HeaderName.PredefinedHeaderNames.RequestTarget);
                interceptedSettings.Headers.ElementAt(1).Should().Be(HeaderName.PredefinedHeaderNames.Date);
            }

            [Fact]
            public void ExcludesEmptyHeaderNames() {
                _settings.Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.Empty, 
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Expires,
                    HeaderName.Empty, 
                    new HeaderName("reco_app_id")
                };
                
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => call.GetArgument<HeaderName>(0) + ",");

                var actual = _sut.Compose(_httpRequest, _settings);

                var expected = "(request-target),date,(expires),reco_app_id,";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void ComposesStringOutOfAllRequestedHeaders() {
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => call.GetArgument<HeaderName>(0) + ",");

                var actual = _sut.Compose(_httpRequest, _settings);

                var expected = "(request-target),date,(expires),reco_app_id,";
                actual.Should().Be(expected);
            }
        }
    }
}