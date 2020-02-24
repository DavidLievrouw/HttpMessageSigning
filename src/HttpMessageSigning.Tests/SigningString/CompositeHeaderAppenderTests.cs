using System;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class CompositeHeaderAppenderTests {
        private readonly IHeaderAppender _defaultHeaderAppender;
        private readonly IHeaderAppender _requestTargetHeaderAppender;
        private readonly IHeaderAppender _createdHeaderAppender;
        private readonly IHeaderAppender _expiresHeaderAppender;
        private readonly IHeaderAppender _dateHeaderAppender;
        private readonly IHeaderAppender _digestHeaderAppender;
        private readonly CompositeHeaderAppender _sut;

        public CompositeHeaderAppenderTests() {
            FakeFactory.Create(
                out _dateHeaderAppender,
                out _createdHeaderAppender,
                out _defaultHeaderAppender,
                out _expiresHeaderAppender,
                out _requestTargetHeaderAppender,
                out _digestHeaderAppender);
            _sut = new CompositeHeaderAppender(
                _defaultHeaderAppender,
                _requestTargetHeaderAppender,
                _createdHeaderAppender,
                _expiresHeaderAppender,
                _dateHeaderAppender,
                _digestHeaderAppender);
        }

        public class BuildStringToAppend : CompositeHeaderAppenderTests {
            public BuildStringToAppend() {
                A.CallTo(() => _createdHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created))
                    .Returns("{known-created}");
                A.CallTo(() => _dateHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Date))
                    .Returns("{known-date}");
                A.CallTo(() => _expiresHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires))
                    .Returns("{known-expires}");
                A.CallTo(() => _requestTargetHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget))
                    .Returns("{known-request-target}");
                A.CallTo(() => _digestHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Digest))
                    .Returns("{known-digest}");
                A.CallTo(() => _defaultHeaderAppender.BuildStringToAppend(A<HeaderName>._))
                    .ReturnsLazily(call => $"{{{call.GetArgument<HeaderName>(0)}}}");
            }

            [Fact]
            public void WhenHeaderIsEmpty_ThrowsHttpMessageSigningValidationException() {
                Action act = () => _sut.BuildStringToAppend(HeaderName.Empty);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }

            [Fact]
            public void WhenHeaderIsRequestTarget_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "(request-target)");
                actual.Should().Be("{known-request-target}");
            }

            [Fact]
            public void WhenHeaderIsCreated_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "(created)");
                actual.Should().Be("{known-created}");
            }

            [Fact]
            public void WhenHeaderIsExpires_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "(expires)");
                actual.Should().Be("{known-expires}");
            }

            [Fact]
            public void WhenHeaderIsDate_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "date");
                actual.Should().Be("{known-date}");
            }
            
            [Fact]
            public void WhenHeaderIsDigest_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "digest");
                actual.Should().Be("{known-digest}");
            }

            [Fact]
            public void WhenHeaderIsSomethingElse_ReturnsResultFromTheDefaultAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "dalion-test");
                actual.Should().Be("{dalion-test}");
            }
        }
    }
}