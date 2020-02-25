using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace Dalion.HttpMessageSigning.Validation {
    public class SignatureParserTests {
        private readonly SignatureParser _sut;

        public SignatureParserTests() {
            _sut = new SignatureParser();
        }

        public class Parse : SignatureParserTests {
            private readonly DefaultHttpRequest _request;
            private readonly long _nowEpoch;
            private readonly long _expiresEpoch;
            private readonly DateTimeOffset _now;
            private readonly DateTimeOffset _expires;

            public Parse() {
                _request = new DefaultHttpRequest(new DefaultHttpContext());
                _now = new DateTimeOffset(2020, 2, 25, 10, 29, 29, TimeSpan.Zero);
                _expires = _now.AddMinutes(10);
                _nowEpoch = _now.ToUnixTimeSeconds();
                _expiresEpoch = _expires.ToUnixTimeSeconds();
            }

            private string Compose(string keyId = null, string algorithm = null, string created = null, string expires = null, string headers = null, string sig = null) {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(keyId)) parts.Add("keyId=\"" + keyId + "\"");
                if (!string.IsNullOrEmpty(algorithm)) parts.Add("algorithm=\"" + algorithm + "\"");
                if (!string.IsNullOrEmpty(created)) parts.Add("created=" + created);
                if (!string.IsNullOrEmpty(expires)) parts.Add("expires=" + expires);
                if (!string.IsNullOrEmpty(headers)) parts.Add("headers=\"" + headers + "\"");
                if (!string.IsNullOrEmpty(sig)) parts.Add(",signature=\"" + sig + "\"");
                return string.Join(",", parts);
            }

            private void SetHeader(HttpRequest request, string keyId = null, string algorithm = null, string created = null, string expires = null, string headers = null,
                string sig = null) {
                var param = Compose(keyId, algorithm, created, expires, headers, sig);
                _request.Headers["Authorization"] = "Signature " + param;
            }

            [Fact]
            public void WhenRequestIsNull_ThrowsArgumentNullException() {
                Action act = () => _sut.Parse(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenRequestHasNoAuthorizationHeader_ThrowsSignatureValidationException() {
                _request.Headers.Clear();

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureValidationException>();
            }

            [Fact]
            public void WhenRequestHasAnInvalidAuthorizationHeader_ThrowsSignatureValidationException() {
                _request.Headers["Authorization"] = "{nonsense}";

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureValidationException>();
            }

            [Fact]
            public void WhenRequestHasAnAuthorizationHeaderForAnotherScheme_ThrowsSignatureValidationException() {
                _request.Headers["Authorization"] = "Custom abc123";

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureValidationException>();
            }

            [Fact]
            public void WhenRequestHasAnAuthorizationHeaderWithoutParam_ThrowsSignatureValidationException() {
                _request.Headers["Authorization"] = "Signature ";

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureValidationException>();
            }

            [Fact]
            public void GivenACompleteParam_ParsesAllProperties() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==");

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
                    String = "xyz123=="
                };
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void WhenCreatedIsNotSpecified_SetsCreatedToNull() {
                SetHeader(_request, "app1", "rsa-sha256", null, _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==");

                var actual = _sut.Parse(_request);

                actual.Created.Should().BeNull();
            }

            [Fact]
            public void WhenCreatedIsNotAnEpoch_SetsCreatedToNull() {
                SetHeader(_request, "app1", "rsa-sha256", "some invalid value", _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==");

                var actual = _sut.Parse(_request);

                actual.Created.Should().BeNull();
            }

            [Fact]
            public void WhenExpiresIsNotSpecified_SetsExpiresToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), null, "(request-target) date content-length", "xyz123==");

                var actual = _sut.Parse(_request);

                actual.Expires.Should().BeNull();
            }

            [Fact]
            public void WhenExpiresIsNotAnEpoch_SetsExpiresToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), "some invalid value", "(request-target) date content-length", "xyz123==");

                var actual = _sut.Parse(_request);

                actual.Expires.Should().BeNull();
            }

            [Fact]
            public void WhenAlgorithmIsNotSpecified_SetsAlgorithmToNull() {
                SetHeader(_request, "app1", null, _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==");

                var actual = _sut.Parse(_request);

                actual.Algorithm.Should().BeNull();
            }

            [Fact]
            public void WhenHeadersIsNotSpecified_SetsHeadersToNull() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), null, "xyz123==");

                var actual = _sut.Parse(_request);

                actual.Headers.Should().BeNull();
            }

            [Fact]
            public void WhenKeIdIsNotSpecified_ThrowsSignatureValidationException() {
                SetHeader(_request, null, "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", "xyz123==");

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureValidationException>();
            }

            [Fact]
            public void WhenStringNotSpecified_ThrowsSignatureValidationException() {
                SetHeader(_request, "app1", "rsa-sha256", _nowEpoch.ToString(), _expiresEpoch.ToString(), "(request-target) date content-length", null);

                Action act = () => _sut.Parse(_request);

                act.Should().Throw<SignatureValidationException>();
            }
        }
    }
}