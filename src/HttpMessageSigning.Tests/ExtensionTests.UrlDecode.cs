using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class UrlDecode : ExtensionTests {
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
            public void GivenSegmentedEncodedInput_ReturnsDecoded() {
                const string encoded = "/api/test/%7BBrooks%7D%20was%20here/create";
                var actual = encoded.UrlDecode();
                actual.Should().Be("/api/test/{Brooks} was here/create");
            }
            
            [Fact]
            public void DoesNotRedecodeUnencodedSegments() {
                const string encoded = "/api/{Brooks} was here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.";
                var actual = encoded.UrlDecode();
                actual.Should().Be("/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.");
            }
        }
    }
}