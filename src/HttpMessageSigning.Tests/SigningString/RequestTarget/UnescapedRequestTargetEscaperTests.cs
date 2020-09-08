using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    public class UnescapedRequestTargetEscaperTests {
        private readonly UnescapedRequestTargetEscaper _sut;

        public UnescapedRequestTargetEscaperTests() {
            _sut = new UnescapedRequestTargetEscaper();
        }

        public class Escape : UnescapedRequestTargetEscaperTests {
            private readonly RequestTargetEscaping _escaping;

            public Escape() {
                _escaping = RequestTargetEscaping.Unescaped;
            }

            [Theory]
            [InlineData(RequestTargetEscaping.OriginalString)]
            [InlineData(RequestTargetEscaping.RFC3986)]
            [InlineData(RequestTargetEscaping.RFC2396)]
            [InlineData((RequestTargetEscaping)(-99))]
            public void GivenUnsupportedEscapingOption_ThrowsArgumentException(RequestTargetEscaping escaping) {
                var requestTarget = new Uri("https://dalion.eu/api/create", UriKind.Absolute);
                Action act = () => _sut.Escape(requestTarget, escaping);
                act.Should().Throw<ArgumentException>();
            }
            
            [Fact]
            public void GivenNullRequestTarget_ThrowsArgumentNullException() {
                Action act = () => _sut.Escape(null, _escaping);
                act.Should().Throw<ArgumentNullException>();
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
            public void GivenAbsoluteUri_UnescapesRFC3986EscapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D", 
                    UriKind.Absolute);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenAbsoluteUri_UnescapesRFC2396EscapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D", 
                    UriKind.Absolute);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenAbsoluteUri_UnescapesPartiallyEscapedUri() {
                var requestTarget = new Uri(
                    "https://dalion.eu/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D", 
                    UriKind.Absolute);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenAbsoluteUri_DoesNotChangeAlreadyUnescapedUri() {
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
            public void GivenRelativeUri_UnescapesRFC3986EscapedUri() {
                var requestTarget = new Uri(
                    "/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D", 
                    UriKind.Relative);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenRelativeUri_UnescapesRFC2396EscapedUri() {
                var requestTarget = new Uri(
                    "/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D", 
                    UriKind.Relative);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenRelativeUri_UnescapesPartiallyEscapedUri() {
                var requestTarget = new Uri(
                    "/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D", 
                    UriKind.Relative);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }

            [Fact]
            public void GivenRelativeUri_DoesNotChangeAlreadyUnescapedUri() {
                var requestTarget = new Uri(
                    "/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}", 
                    UriKind.Relative);
                
                var actual = _sut.Escape(requestTarget, _escaping);

                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={Brooks}");
            }
        }
    }
}