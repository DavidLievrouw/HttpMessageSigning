using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.SigningString.RequestTarget;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class HeaderAppenderFactoryTests {
        private readonly IRequestTargetEscaper _requestTargetEscaper;
        private readonly HeaderAppenderFactory _sut;

        public HeaderAppenderFactoryTests() {
            FakeFactory.Create(out _requestTargetEscaper);
            _sut = new HeaderAppenderFactory(_requestTargetEscaper);
        }

        public class Create : HeaderAppenderFactoryTests {
            private readonly HttpRequestForSigning _httpRequest;
            private readonly DateTimeOffset _timeOfComposing;
            private readonly TimeSpan _expires;
            private readonly RequestTargetEscaping _requestTargetEscaping;

            public Create() {
                _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _expires = TimeSpan.FromMinutes(5);
                _httpRequest = new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = "http://dalion.eu/api/resource/id1".ToUri()
                };
                _requestTargetEscaping = RequestTargetEscaping.RFC3986;
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(null, _requestTargetEscaping, _timeOfComposing, _expires);
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.Create(_httpRequest, _requestTargetEscaping, _timeOfComposing, _expires);
                actual.Should().NotBeNull().And.BeAssignableTo<CompositeHeaderAppender>();
            }
        }
    }
}