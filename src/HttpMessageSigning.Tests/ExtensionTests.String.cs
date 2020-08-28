using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForString : ExtensionTests {
            public class UrlDecode : ForString {
                [Fact]
                [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                public void GivenNullInput_ReturnsNull() {
                    string nullString = null;
                    var actual = nullString.UrlDecode();
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenInputThatDoesNotRequireDecoding_ReturnsInput() {
                    const string unencoded = "Brooks was here";
                    var actual = unencoded.UrlDecode();
                    actual.Should().Be(unencoded);
                }

                [Fact]
                public void GivenEncodedInput_ReturnsDecoded() {
                    const string encoded = "%7BBrooks%7D%20was%20here";
                    var actual = encoded.UrlDecode();
                    actual.Should().Be("{Brooks} was here");
                }

                [Fact]
                public void AlsoDecodesQueryString() {
                    const string encoded = "%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D";
                    var actual = encoded.UrlDecode();
                    actual.Should().Be("{Brooks} was here?query string={Brooks}");
                }

                [Fact]
                public void AlsoDecodesQueryStringWithoutValue() {
                    const string encoded = "%7BBrooks%7D%20was%20here?query%20string";
                    var actual = encoded.UrlDecode();
                    actual.Should().Be("{Brooks} was here?query string");
                }

                [Fact]
                public void GivenSegmentedEncodedInput_ReturnsDecoded() {
                    const string encoded = "/api/test/%7BBrooks%7D%20was%20here/create";
                    var actual = encoded.UrlDecode();
                    actual.Should().Be("/api/test/{Brooks} was here/create");
                }

                [Fact]
                public void CanDecodeAbsoluteUrl() {
                    const string encoded = "https://httpbin.org/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D";
                    var actual = encoded.UrlDecode();
                    actual.Should().Be("https://httpbin.org/api/{Brooks} was here?query string={Brooks}");
                }

                [Fact]
                public void DoesNotReDecodeUnencodedSegments() {
                    const string encoded = "/api/{Brooks} was here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.";
                    var actual = encoded.UrlDecode();
                    actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.");
                }
            }
            
            public class UrlEncode : ForString {
                [Fact]
                [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                public void GivenNullInput_ReturnsNull() {
                    string nullString = null;
                    var actual = nullString.UrlEncode();
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenRelativeStringInputThatDoesNotRequireEncoding_ReturnsInput() {
                    var unencoded = "BrooksWasHere";
                    var actual = unencoded.UrlEncode();
                    actual.Should().Be(unencoded);
                }

                [Fact]
                public void GivenAbsoluteStringInputThatDoesNotRequireEncoding_ReturnsAbsoluteString() {
                    var unencoded = "https://dalion.eu/api";
                    var actual = unencoded.UrlEncode();
                    actual.Should().Be(unencoded);
                }

                [Fact]
                public void GivenDecodedRelativeStringInput_ReturnsEncoded() {
                    var encoded = "{Brooks} was here";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void GivenDecodedAbsoluteStringInput_ReturnsEncoded() {
                    var encoded = "https://dalion.eu:9000/{Brooks} was here";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("https://dalion.eu:9000/%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void GivenRelativeStringInput_ReturnsEncoded() {
                    var encoded = "/api/{Brooks} was here/create";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("/api/%7BBrooks%7D%20was%20here/create");
                }

                [Fact]
                public void GivenAbsoluteStringInput_ReturnsEncoded() {
                    var encoded = "https://dalion.eu/api/{Brooks} was here";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here");
                }

                [Fact]
                public void AlsoEncodesQueryString() {
                    var encoded = "https://dalion.eu/api/{Brooks} was here?query string={Brooks}";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string=%7BBrooks%7D");
                }

                [Fact]
                public void AlsoEncodesQueryStringWithoutValue() {
                    var encoded = "https://dalion.eu/api/{Brooks} was here?query string";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here?query%20string");
                }

                [Fact]
                public void DoesNotEncodeEncodedStringAgain() {
                    var encoded = "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%20string=%7BBrooks%7D");
                }

                [Fact]
                public void EncodesRFC2396StringToRFC3986() {
                    var encoded = "https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7BBrooks%7D";
                    var actual = encoded.UrlEncode();
                    actual.Should().Be("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D");
                }
            }
        }
    }
}