using System;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class AdditionalSignatureHeadersSetterTests {
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly AdditionalSignatureHeadersSetter _sut;

        public AdditionalSignatureHeadersSetterTests() {
            FakeFactory.Create(out _dateHeaderEnsurer, out _digestHeaderEnsurer);
            _sut = new AdditionalSignatureHeadersSetter(_dateHeaderEnsurer, _digestHeaderEnsurer);
        }

        public class AddMissingRequiredHeadersForSignature : AdditionalSignatureHeadersSetterTests {
            private readonly DateTimeOffset _timeOfSigning;
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;

            public AddMissingRequiredHeadersForSignature() {
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _settings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    ClientKey = new ClientKey {
                        Id = new KeyId("client1"),
                        Secret = new Secret("s3cr3t")
                    },
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    },
                    DigestHashAlgorithm = HashAlgorithm.SHA256
                };
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.AddMissingRequiredHeadersForSignature(null, _settings, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.AddMissingRequiredHeadersForSignature(_httpRequest, null, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsHttpMessageSigningValidationException() {
                _settings.ClientKey = null; // Make invalid
                Func<Task> act = () => _sut.AddMissingRequiredHeadersForSignature(_httpRequest, _settings, _timeOfSigning);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public async Task EnsuresDateHeader() {
                await _sut.AddMissingRequiredHeadersForSignature(_httpRequest, _settings, _timeOfSigning);
                A.CallTo(() => _dateHeaderEnsurer.EnsureHeader(_httpRequest, _settings, _timeOfSigning)).MustHaveHappened();
            }

            [Fact]
            public async Task EnsuresDigestHeader() {
                await _sut.AddMissingRequiredHeadersForSignature(_httpRequest, _settings, _timeOfSigning);
                A.CallTo(() => _digestHeaderEnsurer.EnsureHeader(_httpRequest, _settings, _timeOfSigning)).MustHaveHappened();
            }
        }
    }
}