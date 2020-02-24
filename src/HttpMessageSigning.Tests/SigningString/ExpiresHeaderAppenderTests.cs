using System;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class ExpiresHeaderAppenderTests {
        private readonly SigningSettings _settings;
        private readonly ExpiresHeaderAppender _sut;
        private readonly ISystemClock _systemClock;

        public ExpiresHeaderAppenderTests() {
            FakeFactory.Create(out _systemClock);
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
            _sut = new ExpiresHeaderAppender(_systemClock, _settings);
        }

        public class BuildStringToAppend : ExpiresHeaderAppenderTests {
            private readonly DateTimeOffset _now;

            public BuildStringToAppend() {
                _now = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                A.CallTo(() => _systemClock.UtcNow).Returns(_now.UtcDateTime);
            }

            [Fact]
            public void ReturnsExpectedString() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\n(expires): 1582539914";
                actual.Should().Be(expected);
            }
        }
    }
}