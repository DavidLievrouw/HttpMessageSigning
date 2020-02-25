using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class ExpiresHeaderAppenderTests {
        private readonly SigningSettings _settings;
        private readonly ExpiresHeaderAppender _sut;
        private readonly DateTimeOffset _timeOfComposing;

        public ExpiresHeaderAppenderTests() {
            _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
            _settings = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                ClientKey = new ClientKey {
                    Id = new KeyId("client1"),
                    Secret = new HMACSecret("s3cr3t")
                },
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Expires,
                    new HeaderName("dalion_app_id")
                }
            };
            _sut = new ExpiresHeaderAppender(_settings, _timeOfComposing);
        }

        public class BuildStringToAppend : ExpiresHeaderAppenderTests {
            [Fact]
            public void ReturnsExpectedString() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(expires): 1582539914";
                actual.Should().Be(expected);
            }
        }
    }
}