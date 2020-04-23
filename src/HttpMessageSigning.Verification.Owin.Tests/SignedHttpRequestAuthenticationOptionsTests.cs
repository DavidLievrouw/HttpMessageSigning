using System;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class SignedHttpRequestAuthenticationOptionsTests {
        private readonly SignedHttpRequestAuthenticationOptions _sut;

        public SignedHttpRequestAuthenticationOptionsTests() {
            _sut = new SignedHttpRequestAuthenticationOptions {
                Realm = "UnitTests",
                Scheme = "TestScheme",
                RequestSignatureVerifier = A.Fake<IRequestSignatureVerifier>()
            };
        }

        public class Validate : SignedHttpRequestAuthenticationOptionsTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyScheme_ThrowsValidationException(string nullOrEmpty) {
                _sut.Scheme = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyRealm_ThrowsValidationException(string nullOrEmpty) {
                _sut.Realm = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void GivenNullSignatureVerifier_ThrowsValidationException() {
                _sut.RequestSignatureVerifier = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenSchemeContainsSpace_ThrowsInvalidOperationException() {
                _sut.Scheme = "With Space";
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void GivenValidOptions_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}