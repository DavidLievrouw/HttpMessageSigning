using System.Net.Http;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class RequestTargetHeaderAppenderTests {
        private readonly HttpRequestForSigning _httpRequest;
        private readonly RequestTargetHeaderAppender _sut;

        public RequestTargetHeaderAppenderTests() {
            _httpRequest = new HttpRequestForSigning {
                Method = HttpMethod.Post,
                RequestUri = "/api/resource/id1".ToUri()
            };
            _sut = new RequestTargetHeaderAppender(_httpRequest);
        }

        public class BuildStringToAppend : RequestTargetHeaderAppenderTests {
            [Fact]
            public void WritesLowercaseMethod() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                actual.Should().Contain("post");
            }

            [Fact]
            public void DoesNotTouchCasingOfPath() {
                _httpRequest.RequestUri = "/Api/resource/ID1".ToUri();

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                actual.Should().Contain("/Api/resource/ID1");
            }

            [Fact]
            public void ReturnsExpectedString() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                var expected = "\n(request-target): post /api/resource/id1";
                actual.Should().Be(expected);
            }
        }
    }
}