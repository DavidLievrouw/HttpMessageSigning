using System;
using System.Net.Http;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class SigningStringComposerTests {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;
        private readonly SigningStringComposer _sut;

        public SigningStringComposerTests() {
            FakeFactory.Create(out _headerAppenderFactory);
            _sut = new SigningStringComposer(_headerAppenderFactory);
        }

        public class Compose : SigningStringComposerTests {
            private readonly HttpRequestForSigning _httpRequest;
            private readonly HeaderName[] _headerNames;
            private readonly IHeaderAppender _headerAppender;
            private readonly DateTimeOffset _timeOfComposing;
            private readonly TimeSpan _expires;

            public Compose() {
                _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _expires = TimeSpan.FromMinutes(5);
                _httpRequest = new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _headerNames = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    new HeaderName("dalion_app_id")
                };

                FakeFactory.Create(out _headerAppender);
                A.CallTo(() => _headerAppenderFactory.Create(_httpRequest, _timeOfComposing, _expires))
                    .Returns(_headerAppender);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(null, _headerNames, _timeOfComposing, _expires);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullHeaders_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(_httpRequest, null, _timeOfComposing, _expires);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ExcludesEmptyHeaderNames() {
                var headerNames = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.Empty, 
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.Empty, 
                    new HeaderName("dalion_app_id")
                };
                
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => call.GetArgument<HeaderName>(0) + ",");

                var actual = _sut.Compose(_httpRequest, headerNames, _timeOfComposing, _expires);

                var expected = "(request-target),date,dalion_app_id,";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void ComposesStringOutOfAllRequestedHeaders() {
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => "\n" + call.GetArgument<HeaderName>(0) + ",");

                var actual = _sut.Compose(_httpRequest, _headerNames, _timeOfComposing, _expires);

                var expected = "(request-target),\ndate,\ndalion_app_id,";
                actual.Should().Be(expected);
            }
        }
    }
}