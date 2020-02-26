using System;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class HeaderAppenderFactoryTests {
        private readonly HeaderAppenderFactory _sut;

        public HeaderAppenderFactoryTests() {
            _sut = new HeaderAppenderFactory();
        }

        public class Create : HeaderAppenderFactoryTests {
            private readonly HttpRequestForSigning _httpRequest;
            private readonly DateTimeOffset _timeOfComposing;
            private readonly TimeSpan _expires;

            public Create() {
                _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _expires = TimeSpan.FromMinutes(5);
                _httpRequest = new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(null, _timeOfComposing, _expires);
                act.Should().Throw<ArgumentNullException>();
            }


            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.Create(_httpRequest, _timeOfComposing, _expires);
                actual.Should().NotBeNull().And.BeAssignableTo<CompositeHeaderAppender>();
            }
        }
    }
}