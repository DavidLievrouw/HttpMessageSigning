using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForHeaderName : ExtensionTests {
            public class ToSanitizedHttpHeaderName : ForHeaderName {
                [Fact]
                public void GivenEmptyHeaderName_ReturnsExpectedResult() {
                    var actual = HeaderName.Empty.ToSanitizedHttpHeaderName();
                    actual.Should().NotBeNull().And.BeEmpty();
                }

                [Fact]
                public void GivenCreatedHeader_ReturnsExpectedResult() {
                    var actual = HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName();
                    actual.Should().Be("Signature-Created");
                }

                [Fact]
                public void GivenExpiresHeader_ReturnsExpectedResult() {
                    var actual = HeaderName.PredefinedHeaderNames.Expires.ToSanitizedHttpHeaderName();
                    actual.Should().Be("Signature-Expires");
                }

                [Fact]
                public void GivenExpiresRequestTargetHeader_ReturnsExpectedResult() {
                    var actual = HeaderName.PredefinedHeaderNames.RequestTarget.ToSanitizedHttpHeaderName();
                    actual.Should().Be("request-target");
                }

                [Fact]
                public void TrimsBracesFromHeaderName() {
                    var actual = new HeaderName("(Something-Something)").ToSanitizedHttpHeaderName();
                    actual.Should().Be("Something-Something");
                }

                [Fact]
                public void ReturnsTheValueOfTheHeaderName() {
                    var actual = new HeaderName("Dalion-App-Id").ToSanitizedHttpHeaderName();
                    actual.Should().Be("Dalion-App-Id");
                }

                [Fact]
                public void DoesNotChangeCasing() {
                    var actual = new HeaderName("Dalion-APP-Id").ToSanitizedHttpHeaderName();
                    actual.Should().Be("Dalion-APP-Id");
                }
            }
        }
    }
}