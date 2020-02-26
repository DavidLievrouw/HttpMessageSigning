using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RegisteredSignerSettingsStoreTests {
        private readonly IEnumerable<RegisteredSigningSettings> _registeredSigningSettings;
        private readonly RegisteredSignerSettingsStore _sut;

        public RegisteredSignerSettingsStoreTests() {
            _registeredSigningSettings = new[] {
                new RegisteredSigningSettings(new KeyId("client1"), new SigningSettings {DigestHashAlgorithm = HashAlgorithmName.SHA384}),
                new RegisteredSigningSettings(new KeyId("client2"), new SigningSettings {DigestHashAlgorithm = HashAlgorithmName.SHA512})
            };
            _sut = new RegisteredSignerSettingsStore(_registeredSigningSettings);
        }

        public class Get : RegisteredSignerSettingsStoreTests {
            [Fact]
            public void WhenKeyIdIsEmpty_ThrowsArgumentException() {
                Action act = () => _sut.Get(KeyId.Empty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void WhenKeyIdIsNotRegistered_ThrowsInvalidOperationException() {
                Action act = () => _sut.Get(new KeyId("IDontExist"));
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void WhenKeyIdIsRegistered_ReturnsCorrespondingSettings() {
                var keyId = new KeyId("client2");

                var actual = _sut.Get(keyId);

                actual.Should().Be(_registeredSigningSettings.Single(_ => _.KeyId == keyId).SigningSettings);
            }

            [Fact]
            public void WhenKeyIdIsRegistered_ReturnsCorrespondingSettings_WithFilledInKeyId() {
                var keyId = new KeyId("client2");

                var actual = _sut.Get(keyId);

                actual.KeyId.Should().Be(keyId);
            }
        }
    }
}