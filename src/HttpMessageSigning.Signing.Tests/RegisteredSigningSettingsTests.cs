using System;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RegisteredSigningSettingsTests {
        private readonly KeyId _keyId;
        private readonly SigningSettings _signingSettings;
        private readonly RegisteredSigningSettings _sut;
        private readonly ISignatureAlgorithm _signatureAlgorithm;

        public RegisteredSigningSettingsTests() {
            _keyId = new KeyId("abc123");
            _signatureAlgorithm = A.Fake<ISignatureAlgorithm>();
            _signingSettings = new SigningSettings { KeyId = _keyId, SignatureAlgorithm = _signatureAlgorithm };
            _sut = new RegisteredSigningSettings(_keyId, _signingSettings);
        }

        public class Constructor : RegisteredSigningSettingsTests {
            [Fact]
            public void DoesNotAcceptEmptyKeyIds() {
                Action act = () => new RegisteredSigningSettings(KeyId.Empty, _signingSettings);
                act.Should().Throw<ArgumentException>();
            }
            
            [Fact]
            public void DoesNotAcceptNullSigningSettings() {
                Action act = () => new RegisteredSigningSettings(_keyId, null);
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Fact]
            public void AssignsProperties() {
                _sut.KeyId.Should().Be(_keyId);
                _sut.SigningSettings.Should().Be(_signingSettings);
            }
        }

        public class Dispose : RegisteredSigningSettingsTests {
            [Fact]
            public void DisposesSigningSettings() {
                A.CallTo(() => _signatureAlgorithm.Dispose()).MustNotHaveHappened();
                
                _sut.Dispose();

                A.CallTo(() => _signatureAlgorithm.Dispose()).MustHaveHappened();
            }
        }
    }
}