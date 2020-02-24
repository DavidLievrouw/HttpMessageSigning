using System;
using System.Net.Http;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Composing {
    public class DateHeaderAppenderTests {
        private readonly HttpRequestMessage _httpRequest;
        private readonly DateHeaderAppender _sut;

        public DateHeaderAppenderTests() {
            _httpRequest = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://dalion.eu/api/resource/id1")
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
            public void ReturnsExpectedString() {
                var now = new DateTimeOffset(2020, 2, 24, 10, 20, 14, TimeSpan.FromHours(0));
                _httpRequest.Headers.Add("Date", now.ToString("R"));
                
                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires);

                var expected = "\ndate: Mon, 24 Feb 2020 10:20:14 GMT";
                actual.Should().Be(expected);
            }
        }
    }
}