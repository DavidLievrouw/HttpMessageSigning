#if NETCORE
using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class SignedRequestAuthenticationPostConfigureOptionsTests {
        private readonly SignedRequestAuthenticationPostConfigureOptions _sut;

        public SignedRequestAuthenticationPostConfigureOptionsTests() {
            _sut = new SignedRequestAuthenticationPostConfigureOptions();
        }

        public class PostConfigure : SignedRequestAuthenticationPostConfigureOptionsTests {
            private readonly SignedRequestAuthenticationOptions _options;

            public PostConfigure() {
                _options = new SignedRequestAuthenticationOptions {
                    Realm = "r",
                    Scheme = "s"
                };
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void IfRealmIsNullOrEmpty_ThrowsValidationException(string nullOrEmpty) {
                _options.Realm = nullOrEmpty;
                Action act = () => _sut.PostConfigure("tests", _options);
                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void IfSchemeIsNullOrEmpty_ThrowsValidationException(string nullOrEmpty) {
                _options.Scheme = nullOrEmpty;
                Action act = () => _sut.PostConfigure("tests", _options);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenSchemeContainsSpace_ThrowsValidationException() {
                _options.Scheme = "With Space";
                Action act = () => _sut.PostConfigure("tests", _options);
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void IfRealmAndSchemeAreProvided_DoesNotThrow() {
                Action act = () => _sut.PostConfigure("tests", _options);
                act.Should().NotThrow();
            }
        }
    }
}
#endif