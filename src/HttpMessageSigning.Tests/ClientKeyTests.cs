using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class ClientKeyTests {
        private readonly ClientKey _sut;

        public ClientKeyTests() {
            _sut = new ClientKey {
                Id = new KeyId("client1"),
                Secret = new Secret("s3cr3t")
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
            public void WhenSecretIsEmpty_ThrowsValidationException() {
                _sut.Secret = Secret.Empty;
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