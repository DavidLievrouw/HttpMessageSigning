using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    public class OriginalStringRequestTargetEscaperTests {
        private readonly OriginalStringRequestTargetEscaper _sut;

        public OriginalStringRequestTargetEscaperTests() {
            _sut = new OriginalStringRequestTargetEscaper();
        }

        public class Escape : OriginalStringRequestTargetEscaperTests {
            private readonly RequestTargetEscaping _escaping;

            public Escape() {
                _escaping = RequestTargetEscaping.OriginalString;
            }

            [Fact]
            public void GivenNullRequestTarget_ThrowsArgumentNullException() {
                Action act = () => _sut.Escape(null, _escaping);
                act.Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData(RequestTargetEscaping.RFC3986)]
            [InlineData(RequestTargetEscaping.Unescaped)]
            [InlineData(RequestTargetEscaping.RFC2396)]
            [InlineData((RequestTargetEscaping) (-99))]
            public void GivenUnsupportedEscapingOption_ThrowsArgumentException(RequestTargetEscaping escaping) {
                var requestTarget = new Uri("https://dalion.eu/api/create", UriKind.Absolute);
                Action act = () => _sut.Escape(requestTarget, escaping);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData("")]
            [InlineData("/")]
            public void GivenEmptyRelativeUri_ReturnsRootRequestTarget(string relativeUriString) {
                var requestTarget = new Uri(relativeUriString, UriKind.Relative);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/");
            }
            
            [Fact]
            public void GivenAbsoluteUri_ThatDoesNotNeedEscaping_ReturnsOriginalString() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/create",
                    UriKind.Absolute);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/create");
            }

            [Fact]
            public void GivenAbsoluteUri_StripsHashFromUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/create?query=true#anchor",
                    UriKind.Absolute);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/create?query=true");
            }

            [Fact]
            public void GivenAbsoluteUri_DoesNotChangeRFC3986EscapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D",
                    UriKind.Absolute);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D");
            }

            [Fact]
            public void GivenAbsoluteUri_DoesNotChangeRFC2396EscapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D",
                    UriKind.Absolute);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D");
            }

            [Fact]
            public void GivenAbsoluteUri_DoesNotChangePartiallyEscapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D",
                    UriKind.Absolute);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D");
            }

            [Fact]
            public void GivenAbsoluteUri_DoesNotChangeUnescapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}",
                    UriKind.Absolute);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenRelativeUri_ThatDoesNotNeedEscaping_ReturnsOriginalString() {
                var requestTarget = new Uri(
                    "/api/create",
                    UriKind.Relative);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/create");
            }

            [Fact]
            public void GivenRelativeUri_StripsHashFromUri() {
                var requestTarget = new Uri(
                    "/api/create?query=true#anchor",
                    UriKind.Relative);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/create?query=true");
            }

            [Fact]
            public void GivenRelativeUri_DoesNotChangeRFC3986EscapedUri() {
                var requestTarget = new Uri(
                    "/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D",
                    UriKind.Relative);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D");
            }

            [Fact]
            public void GivenRelativeUri_DoesNotChangeRFC2396EscapedUri() {
                var requestTarget = new Uri(
                    "/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D",
                    UriKind.Relative);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D");
            }

            [Fact]
            public void GivenRelativeUri_DoesNotChangePartiallyEscapedUri() {
                var requestTarget = new Uri(
                    "/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D",
                    UriKind.Relative);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D");
            }

            [Fact]
            public void GivenRelativeUri_DoesNotChangeUnescapedUri() {
                var requestTarget = new Uri(
                    "/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}",
                    UriKind.Relative);

                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }
        }
    }
}