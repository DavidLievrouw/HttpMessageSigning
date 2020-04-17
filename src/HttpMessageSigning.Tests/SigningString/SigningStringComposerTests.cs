using System;
using System.Net.Http;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class SigningStringComposerTests {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;
        private readonly INonceAppender _nonceAppender;
        private readonly SigningStringComposer _sut;

        public SigningStringComposerTests() {
            FakeFactory.Create(out _headerAppenderFactory, out _nonceAppender);
            _sut = new SigningStringComposer(_headerAppenderFactory, _nonceAppender);
        }

        public class Compose : SigningStringComposerTests {
            private readonly HttpRequestForSigning _httpRequest;
            private readonly HeaderName[] _headerNames;
            private readonly IHeaderAppender _headerAppender;
            private readonly DateTimeOffset _timeOfComposing;
            private readonly TimeSpan _expires;
            private readonly string _nonce;

            public Compose() {
                _timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _expires = TimeSpan.FromMinutes(5);
                _httpRequest = new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = "http://dalion.eu/api/resource/id1"
                };
                _headerNames = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    new HeaderName("dalion_app_id")
                };
                _nonce = "abc123";

                FakeFactory.Create(out _headerAppender);
                A.CallTo(() => _headerAppenderFactory.Create(_httpRequest, _timeOfComposing, _expires))
                    .Returns(_headerAppender);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(null, _headerNames, _timeOfComposing, _expires, _nonce);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullHeaders_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(_httpRequest, null, _timeOfComposing, _expires, _nonce);
                act.Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyNonce_DoesNotThrow(string nullOrEmpty) {
                Action act = () => _sut.Compose(_httpRequest, _headerNames, _timeOfComposing, _expires, nullOrEmpty);
                act.Should().NotThrow();
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

                A.CallTo(() => _nonceAppender.BuildStringToAppend(_nonce))
                    .Returns("abc123,");
                
                var actual = _sut.Compose(_httpRequest, headerNames, _timeOfComposing, _expires, _nonce);

                var expected = "(request-target),date,dalion_app_id,abc123,";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void ComposesStringOutOfAllRequestedHeaders() {
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => "\n" + call.GetArgument<HeaderName>(0) + ",");

                A.CallTo(() => _nonceAppender.BuildStringToAppend(_nonce))
                    .Returns("abc123,");
                
                var actual = _sut.Compose(_httpRequest, _headerNames, _timeOfComposing, _expires, _nonce);

                var expected = "(request-target),\ndate,\ndalion_app_id,abc123,";
                actual.Should().Be(expected);
            }
        }
    }
}