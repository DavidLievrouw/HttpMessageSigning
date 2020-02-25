using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class ClientKeyTests {
        private readonly ClientKey _sut;

        public ClientKeyTests() {
            _sut = new ClientKey {
                Id = new KeyId("client1"),
                Secret = new HMACSecret("s3cr3t")
            };
        }

        public class Validate : ClientKeyTests {
            [Fact]
            public void WhenKeyIdIsEmpty_ThrowsValidationException() {
                _sut.Id = KeyId.Empty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenSecretIsNull_ThrowsValidationException() {
                _sut.Secret = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenSecretIsNotSupported_ThrowsValidationException() {
                _sut.Secret = new NotSupportedSecret();
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenSecretIsEmpty_ThrowsValidationException() {
                _sut.Secret = HMACSecret.Empty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}