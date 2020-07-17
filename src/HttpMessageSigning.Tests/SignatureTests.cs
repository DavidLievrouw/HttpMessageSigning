using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SignatureTests {
        private readonly Signature _sut;

        public SignatureTests() {
            _sut = new Signature {
                Algorithm = "rsa-sha256",
                KeyId = "client1",
                String = "xyz123==",
                Created = new DateTimeOffset(2020, 2, 26, 13, 18, 12, TimeSpan.Zero),
                Expires = new DateTimeOffset(2020, 2, 26, 13, 23, 12, TimeSpan.Zero),
                Headers = new[] {(HeaderName) "h1", (HeaderName) "h2"}
            };
        }

        public class Validate : SignatureTests {
            [Fact]
            public void IsIValidatable() {
                Action act = () => ((IValidatable)_sut).Validate();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void GivenEmptyKeyId_ThrowsValidationException() {
                _sut.KeyId = KeyId.Empty;

                Action act = () => _sut.Validate();

                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptySignatureString_ThrowsValidationException(string nullOrEmpty) {
                _sut.String = nullOrEmpty;

                Action act = () => _sut.Validate();

                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenHeadersIsNull_ThrowsValidationException() {
                _sut.Headers = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenHeadersIsEmpty_ThrowsValidationException() {
                _sut.Headers = Array.Empty<HeaderName>();
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
        
        public class GetValidationErrors : SignatureTests {
            [Fact]
            public void IsIValidatable() {
                Action act = () => ((IValidatable)_sut).GetValidationErrors();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void GivenEmptyKeyId_IsInvalid() {
                _sut.KeyId = KeyId.Empty;

                var actual = _sut.GetValidationErrors().ToList();
                
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.KeyId));
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptySignatureString_IsInvalid(string nullOrEmpty) {
                _sut.String = nullOrEmpty;

                var actual = _sut.GetValidationErrors().ToList();
                
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.String));
            }

            [Fact]
            public void WhenHeadersIsNull_IsInvalid() {
                _sut.Headers = null;

                var actual = _sut.GetValidationErrors().ToList();
                
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.Headers));
            }

            [Fact]
            public void WhenHeadersIsEmpty_IsInvalid() {
                _sut.Headers = Array.Empty<HeaderName>();

                var actual = _sut.GetValidationErrors().ToList();
                
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.Headers));
            }
            
            [Fact]
            public void WhenEverythingIsValid_IsValid() {
                Action act = () => _sut.Validate();

                var actual = _sut.GetValidationErrors().ToList();
                
                actual.Should().NotBeNull().And.BeEmpty();
            }
        }

        public class Clone : SignatureTests {
            [Fact]
            public void ReturnsNewInstanceWithExpectedValues() {
                var actual = _sut.Clone();
                actual.Should().NotBe(_sut);
                actual.Should().BeEquivalentTo(_sut);
            }
        }
    }
}