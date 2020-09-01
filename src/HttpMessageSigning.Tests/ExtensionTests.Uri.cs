using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForUri : ExtensionTests {
            public class UriEscape : ForUri {
                [Fact]
                [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                public void GivenNullInput_ReturnsNull() {
                    Uri nullUri = null;
                    var actual = nullUri.UriEscape();
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenRelativeUriInputThatDoesNotRequireEncoding_ReturnsInput() {
                    var unencoded = new Uri("BrooksWasHere", UriKind.Relative);
                    var actual = unencoded.UriEscape();
                    actual.Should().Be(unencoded.OriginalString);
                }

                [Fact]
                public void GivenAbsoluteUriInputThatDoesNotRequireEncoding_ReturnsAbsoluteUri() {
                    var unencoded = new Uri("https://dalion.eu/api", UriKind.Absolute);
                    var actual = unencoded.UriEscape();
                    actual.Should().Be(unencoded.AbsoluteUri);
                }

                [Fact]
                public void GivenUnescapedRelativeUriInput_ReturnsEscaped() {
                    var encoded = new Uri("{Brooks} was here", UriKind.Relative);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void GivenUnescapedAbsoluteUriInput_ReturnsEscaped() {
                    var encoded = new Uri("https://dalion.eu:9000/{Brooks} was here");
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu:9000/%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void GivenRelativeUriInput_ReturnsEscaped() {
                    var encoded = new Uri("/api/{Brooks} was here/create", UriKind.Relative);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create");
                }

                [Fact]
                public void GivenAbsoluteUriInput_ReturnsEscaped() {
                    var encoded = new Uri("https://dalion.eu/api/{Brooks} was here", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void AlsoEscapesQueryString() {
                    var encoded = new Uri("https://dalion.eu/api/{Brooks} was here?query string={Brooks}", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D");
                }

                [Fact]
                public void CorrectlyEscapesMultipleQueryString() {
                    var encoded = new Uri("https://dalion.eu/api/{Brooks} was here?query string={Brooks}&id=42?", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D&id=42%3F");
                }

                [Fact]
                public void AlsoEscapesQueryStringWithoutValue() {
                    var encoded = new Uri("https://dalion.eu/api/{Brooks} was here?query string", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string");
                }

                [Fact]
                public void DropsEmptyQueryString() {
                    var encoded = new Uri("https://dalion.eu/api/{Brooks} was here?", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void DoesNotEscapeEscapedStringAgain() {
                    var encoded = new Uri("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D");
                }

                [Fact]
                public void EscapesRFC2396UriToRFC3986() {
                    var encoded = new Uri("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D", UriKind.Absolute);
                    var actual = encoded.UriEscape();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D");
                }
            }
        }
    }
}