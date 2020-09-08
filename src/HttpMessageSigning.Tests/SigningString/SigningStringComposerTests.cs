using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.TestUtils;
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
            private readonly IHeaderAppender _headerAppender;
            private readonly SigningStringCompositionRequest _compositionRequest;

            public Compose() {
                var timeOfComposing = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                var expires = TimeSpan.FromMinutes(5);
                var httpRequest = new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = "http://dalion.eu/api/resource/id1".ToUri()
                };
                var headerNames = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    new HeaderName("dalion_app_id")
                };
                var requestTargetEscaping = RequestTargetEscaping.Unescaped;
                var nonce = "abc123";

                _compositionRequest = new SigningStringCompositionRequest {
                    Request = httpRequest,
                    Expires = expires,
                    Nonce = nonce,
                    HeadersToInclude = headerNames,
                    RequestTargetEscaping = requestTargetEscaping,
                    TimeOfComposing = timeOfComposing
                };

                FakeFactory.Create(out _headerAppender);
                A.CallTo(() => _headerAppenderFactory.Create(
                        _compositionRequest.Request,
                        _compositionRequest.RequestTargetEscaping,
                        _compositionRequest.TimeOfComposing,
                        _compositionRequest.Expires))
                    .Returns(_headerAppender);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Compose(null);
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Fact]
            public void ExcludesEmptyHeaderNames() {
                _compositionRequest.HeadersToInclude = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.Empty, 
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.Empty, 
                    new HeaderName("dalion_app_id")
                };
                
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => call.GetArgument<HeaderName>(0) + ",");

                A.CallTo(() => _nonceAppender.BuildStringToAppend(_compositionRequest.Nonce))
                    .Returns("abc123,");
                
                var actual = _sut.Compose(_compositionRequest);

                var expected = "(request-target),date,dalion_app_id,abc123,";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void ComposesStringOutOfAllRequestedHeaders() {
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => "\n" + call.GetArgument<HeaderName>(0) + ",");

                A.CallTo(() => _nonceAppender.BuildStringToAppend(_compositionRequest.Nonce))
                    .Returns("abc123,");
                
                var actual = _sut.Compose(_compositionRequest);

                var expected = "(request-target),\ndate,\ndalion_app_id,abc123,";
                actual.Should().Be(expected);
            }
            
            [Fact]
            public void AppendsNonce() {
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => "\n" + call.GetArgument<HeaderName>(0) + ",");

                A.CallTo(() => _nonceAppender.BuildStringToAppend(_compositionRequest.Nonce))
                    .Returns("abc123,");
                
                var actual = _sut.Compose(_compositionRequest);

                actual.Should().Contain(",abc123,");
            }
            
            [Fact]
            public void TrimsWhitespaceFromStart() {
                A.CallTo(() => _headerAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => "\n" + call.GetArgument<HeaderName>(0) + ",");

                A.CallTo(() => _nonceAppender.BuildStringToAppend(_compositionRequest.Nonce))
                    .Returns("abc123,");
                
                var actual = _sut.Compose(_compositionRequest);

                var expected = "(request-target),\ndate,\ndalion_app_id,abc123,";
                actual.Should().Be(expected);
            }
        }
    }
}