using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Canonicalize {
    public class Tests {
        private readonly DateTimeOffset _now;

        public Tests() {
            _now = DateTimeOffset.Now;
        }

        [Fact]
        public async Task CreatesSignatureString() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "date"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = $"date: {_now:R}";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task AddsNewLineBetweenValues() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("default-request", _now),
                Headers = "digest,host"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "digest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=\nhost: example.com";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task ForMultiValueHeaders_ConcatenatesAllValues_SeparatedByAnASCIICommaAndASCIISpace() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("multi-value-header-request", _now),
                Headers = "Multi-value,host"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "multi-value: one, two\nhost: example.com";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task IgnoresHeaderNameCasing() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("ignore-case", _now),
                Headers = "content-length,host,digest"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "content-length: 18\nhost: example.com\ndigest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task ShouldCreateALowercasedListOfHeaderFields_SeparatorByANewLine_InOrderOfHeadersParameter() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("default-request", _now),
                Headers = "content-length,host,digest"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "content-length: 18\nhost: example.com\ndigest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=";
            actual.Should().Be(expected);
        }

        [Fact]
        public void WhenAHeaderIsSpecifiedInTheHeadersParameter_ButIsMissingFromTheMessage_ProducesError() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "not-in-request"
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<HttpMessageSigningException>();
        }

        [Fact]
        public void WhenAMalformedHeaderIsSpecifiedInTheHeadersParameter_ProducesError() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("default-request", _now),
                Headers = "digest=="
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<HttpMessageSigningException>();
        }

        [Fact]
        public async Task WhenTheHeaderValueIsAZeroLengthString_IncludesItAsAnASCIISpace() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("zero-length-header-request", _now),
                Headers = "zero"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "zero: ";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task ShouldLowercaseHeaderNames() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "Connection"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "connection: keep-alive";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task GeneratesStringForRequestTargetAsMethodAndPath() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "(request-target)"
            };

            var actual = await Canonicalizer.Run(options);

            var expected = "(request-target): get /basic/request";
            actual.Should().Be(expected);
        }

        [Fact]
        public void WhenTheHeadersParameterIsEmpty_ProducesError() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("default-request", _now),
                Headers = ""
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<HttpMessageSigningException>();
        }

        [Fact]
        public void WhenTheHeadersParameterIsNull_ProducesError() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("default-request", _now),
                Headers = null
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<HttpMessageSigningException>();
        }

        [Theory]
        [InlineData("rsa-sha1", "rsa")]
        [InlineData("hmac-sha256", "hmac")]
        [InlineData("ecdsa-sha512", "ecdsa")]
        public void ForCertainAlgorithms_CreatedHeaderIsNotAllowedToBePartOfTheSignatureString(string algorithm, string type) {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("created-" + type, _now),
                Headers = "(created)",
                Algorithm = algorithm
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<HttpMessageSigningException>();
        }

        [Theory]
        [InlineData("rsa-sha1", "rsa")]
        [InlineData("hmac-sha256", "hmac")]
        [InlineData("ecdsa-sha512", "ecdsa")]
        public void ForCertainAlgorithms_ExpiresHeaderIsNotAllowedToBePartOfTheSignatureString(string algorithm, string type) {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("created-" + type, _now),
                Headers = "(expires)",
                Algorithm = algorithm
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<HttpMessageSigningException>();
        }

        [Fact]
        public void GivenACreationValueThatIsNotAUnixTime_ProducesError() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "(created)",
                Created = "[not-an-integer]",
                Expires = _now.AddMinutes(5).ToUnixTimeSeconds().ToString()
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void GivenAnExpirationValueThatIsNotAUnixTime_ProducesError() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "(created)",
                Created = _now.ToUnixTimeSeconds().ToString(),
                Expires = "[not-an-integer]"
            };

            Func<Task> act = () => Canonicalizer.Run(options);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task GivenAValidCreatedValue_ReturnsExpectedSignatureString() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "(created)",
                Created = _now.ToUnixTimeSeconds().ToString(),
                Expires = _now.AddMinutes(5).ToUnixTimeSeconds().ToString()
            };

            var actual = await Canonicalizer.Run(options);

            var expected = $"(created): {options.Created}";
            actual.Should().Be(expected);
        }

        [Fact]
        public async Task GivenAValidExpiresValue_ReturnsExpectedSignatureString() {
            var options = new CanonicalizeOptions {
                Message = HttpMessages.HttpMessageGenerator.GenerateMessage("basic-request", _now),
                Headers = "(expires)",
                Created = _now.ToUnixTimeSeconds().ToString(),
                Expires = _now.AddMinutes(5).ToUnixTimeSeconds().ToString()
            };

            var actual = await Canonicalizer.Run(options);

            var expected = $"(expires): {options.Expires}";
            actual.Should().Be(expected);
        }
    }
}