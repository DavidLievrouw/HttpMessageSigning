using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForString : ExtensionTests {
            public class UriUnescape : ForString {
                [Fact]
                [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                public void GivenNullInput_ReturnsNull() {
                    string nullString = null;
                    var actual = nullString.UriUnescape();
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenInputThatDoesNotRequireEscaping_ReturnsInput() {
                    const string unencoded = "Brooks was here";
                    var actual = unencoded.UriUnescape();
                    actual.Should().Be(unencoded);
                }

                [Fact]
                public void GivenEscapedInput_ReturnsUnescaped() {
                    const string encoded = "%7BBrooks%7D%20was%20here";
                    var actual = encoded.UriUnescape();
                    actual.Should().Be("{Brooks} was here");
                }

                [Fact]
                public void AlsoUnescapesQueryString() {
                    const string encoded = "%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D";
                    var actual = encoded.UriUnescape();
                    actual.Should().Be("{Brooks} was here?query string={Brooks}");
                }

                [Fact]
                public void AlsoUnescapesQueryStringWithoutValue() {
                    const string encoded = "%7BBrooks%7D%20was%20here?query%20string";
                    var actual = encoded.UriUnescape();
                    actual.Should().Be("{Brooks} was here?query string");
                }

                [Fact]
                public void GivenSegmentedEscapedInput_ReturnsUnescaped() {
                    const string encoded = "/api/test/%7BBrooks%7D%20was%20here/create";
                    var actual = encoded.UriUnescape();
                    actual.Should().Be("/api/test/{Brooks} was here/create");
                }

                [Fact]
                public void CanUnescapeAbsoluteUrl() {
                    const string encoded = "https://httpbin.org/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D";
                    var actual = encoded.UriUnescape();
                    actual.Should().Be("https://httpbin.org/api/{Brooks} was here?query string={Brooks}");
                }

                [Fact]
                public void DoesNotReUnescapeUnencodedSegments() {
                    const string encoded = "/api/{Brooks} was here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.";
                    var actual = encoded.UriUnescape();
                    actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.");
                }
            }

            public class UriEscape : ForString {
                [Fact]
                public void GivenUnsupportedEscaping_ThrowsArgumentOutOfRangeException() {
                    var unencoded = "BrooksWasHere";
                    var unsupported = (UriEscaping) (-99);
                    Action act = () => unencoded.UriEscape(unsupported);
                    act.Should().Throw<ArgumentOutOfRangeException>();
                }

                public class UriEscapeRFC3986 : UriEscape {
                    [Fact]
                    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                    public void GivenNullInput_ReturnsNull() {
                        string nullString = null;
                        var actual = nullString.UriEscape();
                        actual.Should().BeNull();
                    }

                    [Fact]
                    public void GivenRelativeStringInputThatDoesNotRequireEncoding_ReturnsInput() {
                        var unencoded = "/BrooksWasHere";
                        var actual = unencoded.UriEscape();
                        actual.Should().Be(unencoded);
                    }

                    [Fact]
                    public void GivenRelativeUriInputWithoutSlash_PrependsSlash() {
                        var unencoded = "BrooksWasHere";
                        var actual = unencoded.UriEscape();
                        actual.Should().Be("/BrooksWasHere");
                    }

                    [Fact]
                    public void GivenAbsoluteStringInputThatDoesNotRequireEncoding_ReturnsAbsoluteString() {
                        var unencoded = "https://dalion.eu/api";
                        var actual = unencoded.UriEscape();
                        actual.Should().Be(unencoded);
                    }

                    [Fact]
                    public void GivenUnescapedRelativeStringInput_ReturnsEscaped() {
                        var encoded = "{Brooks} was here";
                        var actual = encoded.UriEscape();
                        actual.Should().Be("/%7BBrooks%7D%20was%20here");
                    }

                    [Fact]
                    public void GivenUnescapedAbsoluteStringInput_ReturnsEscaped() {
                        var encoded = "https://dalion.eu:9000/{Brooks} was here";
                        var actual = encoded.UriEscape();
                        actual.Should().Be("https://dalion.eu:9000/%7BBrooks%7D%20was%20here");
                    }

                    [Fact]
                    public void GivenRelativeStringInput_ReturnsEscaped() {
                        var encoded = "/api/{Brooks} was here/create";
                        var actual = encoded.UriEscape();
                        actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create");
                    }

                    [Fact]
                    public void GivenAbsoluteStringInput_ReturnsEscaped() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here";
                        var actual = encoded.UriEscape();
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here");
                    }

                    [Fact]
                    public void AlsoEscapesQueryString() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here?query string={Brooks}";
                        var actual = encoded.UriEscape();
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D");
                    }

                    [Fact]
                    public void AlsoEscapesQueryStringWithoutValue() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here?query string";
                        var actual = encoded.UriEscape();
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string");
                    }

                    [Fact]
                    public void DoesNotEscapeEscapedStringAgain() {
                        var encoded =
                            "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D";
                        var actual = encoded.UriEscape();
                        actual.Should().Be(
                            "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D");
                    }

                    [Fact]
                    public void EscapesRFC2396StringToRFC3986() {
                        var encoded = "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D";
                        var actual = encoded.UriEscape();
                        actual.Should().Be(
                            "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D");
                    }
                }

                public class UriEscapeRFC2396 : UriEscape {
                    [Fact]
                    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                    public void GivenNullInput_ReturnsNull() {
                        Uri nullUri = null;
                        var actual = nullUri.UriEscape(UriEscaping.RFC2396);
                        actual.Should().BeNull();
                    }

                    [Fact]
                    public void GivenRelativeUriInputThatDoesNotRequireEncoding_ReturnsInput() {
                        var unencoded = "/BrooksWasHere";
                        var actual = unencoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("/BrooksWasHere");
                    }

                    [Fact]
                    public void GivenRelativeUriInputWithoutSlash_PrependsSlash() {
                        var unencoded = "BrooksWasHere";
                        var actual = unencoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("/BrooksWasHere");
                    }

                    [Fact]
                    public void GivenAbsoluteUriInputThatDoesNotRequireEncoding_ReturnsAbsoluteUri() {
                        var unencoded = "https://dalion.eu/api";
                        var actual = unencoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be(unencoded);
                    }

                    [Fact]
                    public void GivenUnescapedRelativeUriInput_ReturnsEscaped() {
                        var encoded = "{Brooks} was here";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("/%7BBrooks%7D%20was%20here");
                    }

                    [Fact]
                    public void GivenUnescapedAbsoluteUriInput_ReturnsEscaped() {
                        var encoded = "https://dalion.eu:9000/{Brooks} was here";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu:9000/%7BBrooks%7D%20was%20here");
                    }

                    [Fact]
                    public void GivenRelativeUriInput_ReturnsEscaped() {
                        var encoded = "/api/{Brooks} was here/create";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create");
                    }

                    [Fact]
                    public void GivenAbsoluteUriInput_ReturnsEscaped() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here");
                    }

                    [Fact]
                    public void AlsoEscapesQueryString() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here?query string={Brooks}";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D");
                    }

                    [Fact]
                    public void CorrectlyEscapesMultipleQueryString() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here?query string={Brooks}&id=42?";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D&id=42?");
                    }

                    [Fact]
                    public void AlsoEscapesQueryStringWithoutValue() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here?query string";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string");
                    }

                    [Fact]
                    public void RetainsEmptyQueryString() {
                        var encoded = "https://dalion.eu/api/{Brooks} was here?";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?");
                    }

                    [Fact]
                    public void DoesNotEscapeEscapedStringAgain() {
                        var encoded = "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D");
                    }

                    [Fact]
                    public void DoesNotChangeRFC2396Uri() {
                        var encoded = "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D");
                    }

                    [Fact]
                    public void EscapesRFC3986UriToRFC2396() {
                        var encoded = "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D";
                        var actual = encoded.UriEscape(UriEscaping.RFC2396);
                        actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query%20string=%7BBrooks%7D");
                    }
                }
            }
        }
    }
}