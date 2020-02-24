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
        private readonly CompositeHeaderAppender _sut;

        public CompositeHeaderAppenderTests() {
            FakeFactory.Create(
                out _dateHeaderAppender,
                out _createdHeaderAppender,
                out _defaultHeaderAppender,
                out _expiresHeaderAppender,
                out _requestTargetHeaderAppender);
            _sut = new CompositeHeaderAppender(
                _defaultHeaderAppender,
                _requestTargetHeaderAppender,
                _createdHeaderAppender,
                _expiresHeaderAppender,
                _dateHeaderAppender);
        }

        public class BuildStringToAppend : CompositeHeaderAppenderTests {
            public BuildStringToAppend() {
                A.CallTo(() => _createdHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Created))
                    .Returns("{created}");
                A.CallTo(() => _dateHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Date))
                    .Returns("{date}");
                A.CallTo(() => _expiresHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Expires))
                    .Returns("{expires}");
                A.CallTo(() => _requestTargetHeaderAppender.BuildStringToAppend(HeaderName.PredefinedHeaderNames.RequestTarget))
                    .Returns("{request-target}");
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
                actual.Should().Be("{request-target}");
            }

            [Fact]
            public void WhenHeaderIsCreated_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "(created)");
                actual.Should().Be("{created}");
            }

            [Fact]
            public void WhenHeaderIsExpires_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "(expires)");
                actual.Should().Be("{expires}");
            }

            [Fact]
            public void WhenHeaderIsDate_ReturnsResultFromThatAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "date");
                actual.Should().Be("{date}");
            }

            [Fact]
            public void WhenHeaderIsSomethingElse_ReturnsResultFromTheDefaultAppender() {
                var actual = _sut.BuildStringToAppend((HeaderName) "dalion-test");
                actual.Should().Be("{dalion-test}");
            }
        }
    }
}