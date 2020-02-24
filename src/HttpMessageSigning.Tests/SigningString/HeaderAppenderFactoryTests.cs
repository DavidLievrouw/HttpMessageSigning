using System;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class HeaderAppenderFactoryTests {
        private readonly HeaderAppenderFactory _sut;

        public HeaderAppenderFactoryTests() {
            _sut = new HeaderAppenderFactory();
        }

        public class Create : HeaderAppenderFactoryTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;
            private readonly DateTimeOffset _timeOfComposing;

            public Create() {
                _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
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
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(null, _settings, _timeOfComposing);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(_httpRequest, null, _timeOfComposing);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.Create(_httpRequest, _settings, _timeOfComposing);
                actual.Should().NotBeNull().And.BeAssignableTo<CompositeHeaderAppender>();
            }
        }
    }
}