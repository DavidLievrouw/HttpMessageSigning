using System;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    public class CompositeRequestTargetEscaperTests {
        private readonly IRequestTargetEscaper _originalStringEscaper;
        private readonly IRequestTargetEscaper _rfc2396Escaper;
        private readonly IRequestTargetEscaper _rfc3986Escaper;
        private readonly CompositeRequestTargetEscaper _sut;
        private readonly IRequestTargetEscaper _unescapedEscaper;

        public CompositeRequestTargetEscaperTests() {
            FakeFactory.Create(out _rfc2396Escaper, out _rfc3986Escaper, out _originalStringEscaper, out _unescapedEscaper);
            _sut = new CompositeRequestTargetEscaper(
                _rfc3986Escaper,
                _rfc2396Escaper,
                _unescapedEscaper,
                _originalStringEscaper);
        }

        public class Escape : CompositeRequestTargetEscaperTests {
            private readonly Uri _requestTarget;

            public Escape() {
                _requestTarget = new Uri("/api/test", UriKind.Relative);
            }

            [Fact]
            public void GivenNullRequestTarget_ThrowsArgumentNullException() {
                Action act = () => _sut.Escape(null, RequestTargetEscaping.RFC3986);
                act.Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData(RequestTargetEscaping.RFC3986)]
            [InlineData(RequestTargetEscaping.RFC2396)]
            [InlineData(RequestTargetEscaping.OriginalString)]
            [InlineData(RequestTargetEscaping.Unescaped)]
            public void CallsCorrectEscaperAndReturnsItsResult(RequestTargetEscaping escaping) {
                A.CallTo(() => _rfc3986Escaper.Escape(_requestTarget, escaping))
                    .Returns("escaped_rfc3986");
                A.CallTo(() => _rfc2396Escaper.Escape(_requestTarget, escaping))
                    .Returns("escaped_rfc2396");
                A.CallTo(() => _originalStringEscaper.Escape(_requestTarget, escaping))
                    .Returns("escaped_originalstring");
                A.CallTo(() => _unescapedEscaper.Escape(_requestTarget, escaping))
                    .Returns("escaped_unescaped");

                var actual = _sut.Escape(_requestTarget, escaping);

                actual.Should().Be("escaped_" + escaping.ToString().ToLower());
            }

            [Fact]
            public void GivenUnsupportedEscapingOption_ThrowsArgumentOutOfRangeException() {
                var unsupported = (RequestTargetEscaping) (-99);
                Action act = () => _sut.Escape(_requestTarget, unsupported);
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }
    }
}