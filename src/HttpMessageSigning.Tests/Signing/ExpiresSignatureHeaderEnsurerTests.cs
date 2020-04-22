using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class ExpiresSignatureHeaderEnsurerTests {
        private readonly ExpiresSignatureHeaderEnsurer _sut;

        public ExpiresSignatureHeaderEnsurerTests() {
            _sut = new ExpiresSignatureHeaderEnsurer();
        }

        public class EnsureHeader : ExpiresSignatureHeaderEnsurerTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly SigningSettings _settings;
            private readonly DateTimeOffset _timeOfSigning;
            private readonly string _expectedHeaderValue;

            public EnsureHeader() {
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.FromHours(1));
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _settings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    KeyId = "client1",
                    SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Created,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    },
                    DigestHashAlgorithm = HashAlgorithmName.SHA256
                };
                _expectedHeaderValue = _timeOfSigning.Add(_settings.Expires).ToUnixTimeSeconds().ToString();
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
            public void WhenExpiresHeaderIsRequired_ButItIsNotInRequest_AddsIt() {
                _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == HeaderName.PredefinedHeaderNames.Expires.ToSanitizedHttpHeaderName() && h.Value.Single() == _expectedHeaderValue);
            }
            
            [Fact]
            public void WhenExpiresHeaderIsNotRequired_AndItIsNotInRequest_DoesNotAddIt() {
                _settings.Headers = _settings.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Expires).ToArray();
                
                _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain(h => h.Key == HeaderName.PredefinedHeaderNames.Expires.ToSanitizedHttpHeaderName());
            }
        }
    }
}