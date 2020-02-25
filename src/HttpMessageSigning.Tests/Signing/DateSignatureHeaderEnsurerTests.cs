using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class DateSignatureHeaderEnsurerTests {
        private readonly DateSignatureHeaderEnsurer _sut;

        public DateSignatureHeaderEnsurerTests() {
            _sut = new DateSignatureHeaderEnsurer();
        }

        public class EnsureHeader : DateSignatureHeaderEnsurerTests {
            private readonly DateTimeOffset _timeOfSigning;
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;

            public EnsureHeader() {
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _settings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    KeyId = new KeyId("client1"),
                    SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithm.SHA512),
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
                Func<Task> act = () => _sut.EnsureHeader(null, _settings, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.EnsureHeader(_httpRequest, null, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WhenRequestAlreadyHasADateValue_DoesNotChangeDateValue() {
                var existingValue = new DateTimeOffset(2020, 2, 24, 20, 14, 35, TimeSpan.Zero);
                _httpRequest.Headers.Date = existingValue;

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Date.Should().Be(existingValue);
            }

            [Fact]
            public async Task WhenRequestDoesNotHaveADateValue_SetsDateValueToTimeOfSigning() {
                _httpRequest.Headers.Date = null;

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Date.Should().Be(_timeOfSigning);
            }
        }
    }
}