using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class CompositeSignatureHeaderEnsurerTests {
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly CompositeSignatureHeaderEnsurer _sut;

        public CompositeSignatureHeaderEnsurerTests() {
            FakeFactory.Create(out _dateHeaderEnsurer, out _digestHeaderEnsurer);
            _sut = new CompositeSignatureHeaderEnsurer(_dateHeaderEnsurer, _digestHeaderEnsurer);
        }

        public class EnsureHeader : CompositeSignatureHeaderEnsurerTests {
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
                    KeyId = "client1",
                    SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    },
                    DigestHashAlgorithm = HashAlgorithmName.SHA256
                };
            }

            [Fact]
            public async Task EnsuresAllHeaders() {
                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                A.CallTo(() => _dateHeaderEnsurer.EnsureHeader(_httpRequest, _settings, _timeOfSigning)).MustHaveHappened();
                A.CallTo(() => _digestHeaderEnsurer.EnsureHeader(_httpRequest, _settings, _timeOfSigning)).MustHaveHappened();
            }
        }
    }
}