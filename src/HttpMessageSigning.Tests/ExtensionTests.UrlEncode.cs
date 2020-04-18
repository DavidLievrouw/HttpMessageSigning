using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class UrlEncode : ExtensionTests {
            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullInput_ReturnsNull() {
                Uri nullUri = null;
                var actual = nullUri.UrlEncode();
                actual.Should().BeNull();
            }

            [Fact]
            public void GivenRelativeUriInputThatDoesNotRequireEncoding_ReturnsInput() {
                var unencoded = new Uri("BrooksWasHere", UriKind.Relative);
                var actual = unencoded.UrlEncode();
                actual.Should().Be(unencoded.OriginalString);
            }

            [Fact]
            public void GivenAbsoluteUriInputThatDoesNotRequireEncoding_ReturnsAbsoluteUri() {
                var unencoded = new Uri("https://dalion.eu/api", UriKind.Absolute);
                var actual = unencoded.UrlEncode();
                actual.Should().Be(unencoded.AbsoluteUri);
            }

            [Fact]
            public void GivenDecodedRelativeUriInput_ReturnsEncoded() {
                var encoded = new Uri("{Brooks} was here", UriKind.Relative);
                var actual = encoded.UrlEncode();
                actual.Should().Be("%7BBrooks%7D%20was%20here");
            }
            
            [Fact]
            public void GivenDecodedAbsoluteUriInput_ReturnsEncoded() {
                var encoded = new Uri("https://dalion.eu:9000/{Brooks} was here");
                var actual = encoded.UrlEncode();
                actual.Should().Be("https://dalion.eu:9000/%7BBrooks%7D%20was%20here");
            }

            [Fact]
            public void GivenRelativeUriInput_ReturnsEncoded() {
                var encoded = new Uri("/api/{Brooks} was here/create", UriKind.Relative);
                var actual = encoded.UrlEncode();
                actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create");
            }

            [Fact]
            public void GivenAbsoluteUriInput_ReturnsEncoded() {
                var encoded = new Uri("https://dalion.eu/api/{Brooks} was here", UriKind.Absolute);
                var actual = encoded.UrlEncode();
                actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here");
            }
            
            [Fact]
            public void DoesNotEncodeEncodedStringAgain() {
                var encoded = new Uri("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David & Partners + Siebe at 100% * co.", UriKind.Absolute);
                var actual = encoded.UrlEncode();
                actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.");
            }
        }
    }
}