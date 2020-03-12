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
                _options = new SignedRequestAuthenticationOptions();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void IfRealmIsNullOrEmpty_ThrowsInvalidOperationException(string nullOrEmpty) {
                _options.Realm = nullOrEmpty;
                Action act = () => _sut.PostConfigure("tests", _options);
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void IfRealmIsProvided_DoesNotThrow() {
                _options.Realm = "Unit tests";
                Action act = () => _sut.PostConfigure("tests", _options);
                act.Should().NotThrow();
            }
        }
    }
}