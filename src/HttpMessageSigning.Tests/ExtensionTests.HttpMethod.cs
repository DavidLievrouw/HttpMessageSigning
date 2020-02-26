using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForHttpMethod : ExtensionTests {
            public class SupportsBody : ForHttpMethod {
                [Fact]
                public void WhenMethodIsNull_ThrowsArgumentNullException() {
                    Action act = () => Extensions.SupportsBody(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Theory]
                [InlineData("GET")]
                [InlineData("TRACE")]
                [InlineData("HEAD")]
                [InlineData("DELETE")]
                public void WhenMethodIsOneOfThoseWithoutBody_ReturnsFalse(string method) {
                    var sut = new System.Net.Http.HttpMethod(method);
                    var actual = sut.SupportsBody();
                    actual.Should().BeFalse();
                }

                [Theory]
                [InlineData("POST")]
                [InlineData("PUT")]
                [InlineData("REPORT")]
                [InlineData("PATCH")]
                public void WhenMethodIsOneOfThoseWithBody_ReturnsTrue(string method) {
                    var sut = new System.Net.Http.HttpMethod(method);
                    var actual = sut.SupportsBody();
                    actual.Should().BeTrue();
                }
            }
        }
    }
}