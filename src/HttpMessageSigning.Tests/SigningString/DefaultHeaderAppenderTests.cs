using System;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class DefaultHeaderAppenderTests {
        private readonly HttpRequestMessage _httpRequest;
        private readonly DefaultHeaderAppender _sut;

        public DefaultHeaderAppenderTests() {
            _httpRequest = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://dalion.eu/api/resource/id1")
            };
            _sut = new DefaultHeaderAppender(_httpRequest);
        }

        public class BuildStringToAppend : DefaultHeaderAppenderTests {
            private readonly HeaderName _headerName;

            public BuildStringToAppend() {
                _headerName = new HeaderName("Dalion-test");
            }

            [Fact]
            public void WhenHeaderIsNotPresent_ReturnsEmptyString() {
                _httpRequest.Headers.Clear();

                var actual = _sut.BuildStringToAppend(_headerName);

                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void WhenHeaderHasAnEmptyValue_ReturnsHeaderWithEmptyValueAccordingToSpec() {
                _httpRequest.Headers.Add("Dalion-test", "");

                var actual = _sut.BuildStringToAppend(_headerName);

                actual.Should().Be("\ndalion-test: ");
            }

            [Fact]
            public void WhenHeaderHasOneValue_ReturnsExpectedString() {
                _httpRequest.Headers.Add("Dalion-test", "forty-two");

                var actual = _sut.BuildStringToAppend(_headerName);

                actual.Should().Be("\ndalion-test: forty-two");
            }

            [Fact]
            public void WhenHeaderHasMultipleValues_ReturnsExpectedString() {
                _httpRequest.Headers.Add("Dalion-test", new[] {"forty-two", "forty-three"});

                var actual = _sut.BuildStringToAppend(_headerName);

                actual.Should().Be("\ndalion-test: forty-two, forty-three");
            }

            [Fact]
            public void StripsWhitespaceFromValues() {
                _httpRequest.Headers.Add("Dalion-test", new[] {" forty-two", " forty-three \t "});

                var actual = _sut.BuildStringToAppend(_headerName);

                actual.Should().Be("\ndalion-test: forty-two, forty-three");
            }

            [Fact]
            public void StripsLineBreaksFromHeaderValues() {
                _httpRequest.Headers.Add("Dalion-test", new[] {" forty-two \n with a linebreak", " forty-three \r\n with a linebreak"});

                var actual = _sut.BuildStringToAppend(_headerName);

                actual.Should().Be("\ndalion-test: forty-two with a linebreak, forty-three with a linebreak");
            }
        }
    }
}