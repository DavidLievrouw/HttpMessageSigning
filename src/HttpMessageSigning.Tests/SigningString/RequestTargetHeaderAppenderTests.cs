using System.Net.Http;
using Dalion.HttpMessageSigning.SigningString.RequestTarget;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class RequestTargetHeaderAppenderTests {
        private readonly HttpRequestForSignatureString _httpRequest;
        private readonly RequestTargetHeaderAppender _sut;
        private readonly IRequestTargetEscaper _requestTargetEscaper;
        private readonly RequestTargetEscaping _requestTargetEscaping;

        public RequestTargetHeaderAppenderTests() {
            FakeFactory.Create(out _requestTargetEscaper);
            _httpRequest = new HttpRequestForSignatureString {
                Method = HttpMethod.Post,
                RequestUri = "/api/resource/id1".ToUri()
            };
            _requestTargetEscaping = RequestTargetEscaping.RFC3986;
            _sut = new RequestTargetHeaderAppender(_httpRequest, _requestTargetEscaping, _requestTargetEscaper);
        }

        public class BuildStringToAppend : RequestTargetHeaderAppenderTests {
            [Fact]
            public void WritesLowercaseMethod() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                actual.Should().Contain("post");
            }

            [Fact]
            public void EscapesRequestTargetAccordingToSettings() {
                A.CallTo(() => _requestTargetEscaper.Escape(_httpRequest.RequestUri, _requestTargetEscaping))
                    .Returns("/api/escaped");

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                actual.Should().Contain(" /api/escaped");
            }

            [Fact]
            public void ReturnsExpectedString() {
                A.CallTo(() => _requestTargetEscaper.Escape(_httpRequest.RequestUri, _requestTargetEscaping))
                    .Returns("/api/escaped");
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                var expected = "\n(request-target): post /api/escaped";
                actual.Should().Be(expected);
            }
        }
    }
}