using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class DateHeaderAppenderTests {
        private readonly HttpRequestForSignatureString _httpRequest;
        private readonly DateHeaderAppender _sut;

        public DateHeaderAppenderTests() {
            _httpRequest = new HttpRequestForSignatureString {
                Method = HttpMethod.Post,
                RequestUri = "http://dalion.eu/api/resource/id1".ToUri()
            };
            _sut = new DateHeaderAppender(_httpRequest);
        }

        public class BuildStringToAppend : DateHeaderAppenderTests {
            [Fact]
            public void WhenRequestDoesNotContainDateHeader_ReturnsEmptyString() {
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);
                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void WhenDateHeaderContainsNonsense_ReturnsEmptyStrings() {
                _httpRequest.Headers["Date"] = "Not even close to being a date";
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);
                
                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void ReturnsExpectedString() {
                var now = new DateTimeOffset(2020, 2, 24, 10, 20, 14, TimeSpan.FromHours(0));
                _httpRequest.Headers["Date"] = now.ToString("R");

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\ndate: Mon, 24 Feb 2020 10:20:14 GMT";
                actual.Should().Be(expected);
            }
        }
    }
}