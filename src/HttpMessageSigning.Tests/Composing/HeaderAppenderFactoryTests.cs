using System;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Composing {
    public class HeaderAppenderFactoryTests {
        private readonly ISystemClock _systemClock;
        private readonly HeaderAppenderFactory _sut;

        public HeaderAppenderFactoryTests() {
            FakeFactory.Create(out _systemClock);
            _sut = new HeaderAppenderFactory(_systemClock);
        }

        public class Create : HeaderAppenderFactoryTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;

            public Create() {
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
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(null, _settings);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(_httpRequest, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.Create(_httpRequest, _settings);
                actual.Should().NotBeNull().And.BeAssignableTo<CompositeHeaderAppender>();
            }
        }
    }
}