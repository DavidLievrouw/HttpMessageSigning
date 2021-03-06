using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class DefaultAuthorizationHeaderExtractorTests {
        private readonly DefaultAuthorizationHeaderExtractor _sut;

        public DefaultAuthorizationHeaderExtractorTests() {
            _sut = new DefaultAuthorizationHeaderExtractor();
        }

        public class Extract : DefaultAuthorizationHeaderExtractorTests {
            private readonly HttpRequest _request;

            public Extract() {
                _request = new DefaultHttpContext().Request;
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.Extract(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenRequestDoesNotHaveAnAuthorizationHeader_ReturnsNull() {
                var actual = _sut.Extract(_request);
                
                actual.Should().BeNull();
            }

            [Fact]
            public void WhenAuthorizationHeaderHasNoParam_ReturnsValueWithoutParam() {
                _request.Headers["Authorization"] = "Signature";
                
                var actual = _sut.Extract(_request);
                
                actual.Scheme.Should().Be("Signature");
                actual.Parameter.Should().BeNull();
            }

            [Fact]
            public void WhenAuthorizationHeaderHasEmptyParam_ReturnsValueWithoutParam() {
                _request.Headers["Authorization"] = "Signature ";
                
                var actual = _sut.Extract(_request);
                
                actual.Scheme.Should().Be("Signature");
                actual.Parameter.Should().BeNull();
            }

            [Fact]
            public void WhenAuthorizationHeaderHasSchemeAndParam_ReturnsExpectedValue() {
                _request.Headers["Authorization"] = "Signature abc123";
                
                var actual = _sut.Extract(_request);
                
                actual.Scheme.Should().Be("Signature");
                actual.Parameter.Should().Be("abc123");
            }
        }
    }
}