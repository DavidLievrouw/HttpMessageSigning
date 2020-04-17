using System;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class RequestTargetHeaderAppenderTests {
        private readonly HttpRequestForSigning _httpRequest;
        private readonly RequestTargetHeaderAppender _sut;

        public RequestTargetHeaderAppenderTests() {
            _httpRequest = new HttpRequestForSigning {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://dalion.eu/api/resource/id1")
            };
            _sut = new RequestTargetHeaderAppender(_httpRequest);
        }

        public class BuildStringToAppend : RequestTargetHeaderAppenderTests {
            [Fact]
            public void OmitsQueryString() {
                _httpRequest.RequestUri = new Uri("http://dalion.eu/api/resource/id1?blah=true");

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                var expected = "\n(request-target): post /api/resource/id1";
                actual.Should().Be(expected);
            }

            [Fact]
            public void CanHandleRelativeUris() {
                _httpRequest.RequestUri = new Uri("/api/resource/id1?blah=true", UriKind.Relative);

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                actual.Should().Contain("/api/resource/id1");
            }

            [Fact]
            public void WritesLowercaseMethod() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                actual.Should().Contain("post");
            }

            [Fact]
            public void DoesNotTouchCasingOfPath() {
                _httpRequest.RequestUri = new Uri("http://dalion.eu/Api/resource/ID1");

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