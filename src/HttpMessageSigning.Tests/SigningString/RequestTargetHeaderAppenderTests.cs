using System;
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

            [Theory]
            [InlineData("POST", "http", "www.example.com/?param=value", "post /?param=value")]
            [InlineData("CUSTOM", "http", "www.example.com/a/b", "custom /a/b")]
            [InlineData("GET", "http", "www.example.com/a", "get /a")]
            [InlineData("GET", "https", "www.example.com", "get /")]
            [InlineData("CONNECT", "http", "server.example.com:80", "connect /")]
            [InlineData("OPTIONS", "", "*", "options *")]
            [InlineData("PUT", "http", "www.example.com:80/foo?param=value&pet=dog", "put /foo?param=value&pet=dog")]
            [InlineData("POST", "", "/?param=value", "post /?param=value")]
            [InlineData("CUSTOM", "", "/a/b", "custom /a/b")]
            [InlineData("GET", "", "/a", "get /a")]
            [InlineData("GET", "", "", "get /")]
            [InlineData("PUT", "", "/foo?param=value&pet=dog", "put /foo?param=value&pet=dog")]
            public void ExamplesFromSpec(string httpMethod, string scheme, string requestUri, string expectedValue) {
                _httpRequest.Method = new HttpMethod(httpMethod);
                var requestUriString = string.IsNullOrEmpty(scheme)
                    ? requestUri
                    : scheme + "://" + requestUri;
                _httpRequest.RequestUri = new Uri(requestUriString, UriKind.RelativeOrAbsolute);

                A.CallTo(() => _requestTargetEscaper.Escape(_httpRequest.RequestUri, _requestTargetEscaping))
                    .ReturnsLazily(call => new RFC3986RequestTargetEscaper().Escape(call.GetArgument<Uri>(0), RequestTargetEscaping.RFC3986));

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget);

                var expected = "\n(request-target): " + expectedValue;
                actual.Should().Be(expected);
            }
        }
    }
}