using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class SignatureParserTests {
        private readonly ILogger<SignatureParser> _logger;
        private readonly SignatureParser _sut;

        public SignatureParserTests() {
            FakeFactory.Create(out _logger);
            _sut = new SignatureParser(_logger);
        }

        public class Parse : SignatureParserTests {
            private readonly HttpRequest _request;
            private readonly long _nowEpoch;
            private readonly long _expiresEpoch;
            private readonly DateTimeOffset _now;
            private readonly DateTimeOffset _expires;

            public Parse() {
                _request = new DefaultHttpContext().Request;
                _now = new DateTimeOffset(2020, 2, 25, 10, 29, 29, TimeSpan.Zero);
                _expires = _now.AddMinutes(10);
                _nowEpoch = _now.ToUnixTimeSeconds();
                _expiresEpoch = _expires.ToUnixTimeSeconds();
            }

            [Fact]
            public void WhenRequestIsNull_ThrowsArgumentNullException() {
                Action act = () => _sut.Parse(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenRequestHasNoAuthorizationHeader_ThrowsSignatureVerificationException() {
                _request.Headers.Clear();

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public void WhenRequestHasAnInvalidAuthorizationHeader_ThrowsSignatureVerificationException() {
                _request.Headers["Authorization"] = "{nonsense}";

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public void WhenRequestHasAnAuthorizationHeaderForAnotherScheme_ThrowsSignatureVerificationException() {
                _request.Headers["Authorization"] = "Custom abc123";

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public void WhenRequestHasAnAuthorizationHeaderWithoutParam_ThrowsSignatureVerificationException() {
                _request.Headers["Authorization"] = "SignedHttpRequest ";

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public void GivenACompleteParam_ParsesAllProperties() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                var expected = new Signature {
                    KeyId = new KeyId("app1"),
                    Algorithm = "rsa-sha256",
                    Created = _now,
                    Expires = _expires,
                    Headers = new[] {
                        new HeaderName("(request-target)"),
                        new HeaderName("date"),
                        new HeaderName("content-length")
                    },
                    String = "xyz123==",
                    Nonce = "abc123"
                };
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void IgnoresAdditionalSettings() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==", "abc123");
                _request.Headers["Authorization"] = _request.Headers["Authorization"] + ",additional=true";

                var actual = _sut.Parse(_request);

                var expected = new Signature {
                    KeyId = new KeyId("app1"),
                    Algorithm = "rsa-sha256",
                    Created = _now,
                    Expires = _expires,
                    Headers = new[] {
                        new HeaderName("(request-target)"),
                        new HeaderName("date"),
                        new HeaderName("content-length")
                    },
                    String = "xyz123==",
                    Nonce = "abc123"
                };
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void WhenCreatedIsNotSpecified_SetsCreatedToNull() {
                SetHeader(_request, "app1", "rsa-sha256", null, _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                actual.Created.Should().BeNull();
            }

            [Fact]
            public void WhenCreatedIsNotAnEpoch_SetsCreatedToNull() {
                SetHeader(_request, "app1", "rsa-sha256", "some invalid value", _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                actual.Created.Should().BeNull();
            }

            [Fact]
            public void WhenExpiresIsNotSpecified_SetsExpiresToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), null, "(request-target) date content-length", "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                actual.Expires.Should().BeNull();
            }

            [Fact]
            public void WhenExpiresIsNotAnEpoch_SetsExpiresToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), "some invalid value", "(request-target) date content-length", "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                actual.Expires.Should().BeNull();
            }

            [Fact]
            public void WhenAlgorithmIsNotSpecified_SetsAlgorithmToNull() {
                SetHeader(_request, "app1", null, _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                actual.Algorithm.Should().BeNull();
            }

            [Fact]
            public void WhenHeadersIsNotSpecified_SetsHeadersToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), null, "xyz123==", "abc123");

                var actual = _sut.Parse(_request);

                actual.Headers.Should().BeNull();
            }

            [Fact]
            public void WhenKeIdIsNotSpecified_ThrowsSignatureVerificationException() {
                SetHeader(_request, null, "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==", "abc123");

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public void WhenStringNotSpecified_ThrowsSignatureVerificationException() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", null, "abc123");

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureVerificationException>();
            }

            [Fact]
            public void WhenNonceIsNotSpecified_SetsNonceToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==");

                var actual = _sut.Parse(_request);
                
                actual.Nonce.Should().BeNull();
            }
            
            private static string Compose(
                string keyId = null,
                string algorithm = null,
                string created = null,
                string expires = null,
                string headers = null,
                string sig = null,
                string nonce = null) {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(keyId)) parts.Add("keyId=\"" + keyId + "\"");
                if (!string.IsNullOrEmpty(algorithm)) parts.Add("algorithm=\"" + algorithm + "\"");
                if (!string.IsNullOrEmpty(created)) parts.Add("created=" + created);
                if (!string.IsNullOrEmpty(expires)) parts.Add("expires=" + expires);
                if (!string.IsNullOrEmpty(headers)) parts.Add("headers=\"" + headers + "\"");
                if (!string.IsNullOrEmpty(nonce)) parts.Add("nonce=\"" + nonce + "\"");
                if (!string.IsNullOrEmpty(sig)) parts.Add("signature=\"" + sig + "\"");
                return string.Join(",", parts);
            }

            private static void SetHeader(
                HttpRequest request,
                string keyId = null,
                string algorithm = null,
                string created = null,
                string expires = null,
                string headers = null,
                string sig = null,
                string nonce = null) {
                var param = Compose(keyId, algorithm, created, expires, headers, sig, nonce);
                request.Headers["Authorization"] = "SignedHttpRequest " + param;
            }
        }
    }
}