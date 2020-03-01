using System;
using System.Reflection;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class HttpRequestSignatureAuthenticationMiddlewareTests {
        private readonly HttpRequestSignatureAuthenticationMiddleware _sut;
        private readonly SignedHttpRequestAuthenticationOptions _options;
        private readonly FakeOwinMiddleware _next;

        public HttpRequestSignatureAuthenticationMiddlewareTests() {
            _options = new SignedHttpRequestAuthenticationOptions {
                Realm = "UnitTests",
                Scheme = "TestScheme",
                RequestSignatureVerifier = A.Fake<IRequestSignatureVerifier>()
            };
            _next = new FakeOwinMiddleware();
            _sut = new HttpRequestSignatureAuthenticationMiddleware(_next, _options);
        }

        public class Constructor : HttpRequestSignatureAuthenticationMiddlewareTests {
            [Fact]
            public void GivenInvalidOptions_ThrowsValidationException() {
                _options.Scheme = null; // Make invalid
                Action act = () => new HttpRequestSignatureAuthenticationMiddleware(_next, _options);
                act.Should().Throw<ValidationException>();
            }
        }

        public class CreateHandler : HttpRequestSignatureAuthenticationMiddlewareTests {
            [Fact]
            public void ReturnsInstanceOfExpectedType() {
                var method = _sut.GetType().GetMethod(nameof(CreateHandler), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var actual = method.Invoke(_sut, Array.Empty<object>());
                actual.Should().NotBeNull().And.BeAssignableTo<SignedHttpRequestAuthenticationHandler>();
            }
        }
    }
}