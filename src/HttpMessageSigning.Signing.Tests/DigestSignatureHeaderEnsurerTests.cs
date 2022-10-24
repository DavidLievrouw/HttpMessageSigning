using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class DigestSignatureHeaderEnsurerTests {
        private readonly IBase64Converter _base64Converter;
        private readonly DigestSignatureHeaderEnsurer _sut;

        public DigestSignatureHeaderEnsurerTests() {
            FakeFactory.Create(out _base64Converter);
            _sut = new DigestSignatureHeaderEnsurer(_base64Converter);
        }

        public class EnsureHeader : DigestSignatureHeaderEnsurerTests {
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
            public async Task GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.EnsureHeader(null, _settings, _timeOfSigning);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }

            [Fact]
            public async Task GivenNullSettings_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.EnsureHeader(_httpRequest, null, _timeOfSigning);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("TRACE")]
            [InlineData("HEAD")]
            [InlineData("DELETE")]
            public async Task WhenMethodDoesNotHaveBody_DoesNotSetDigestHeader(string method) {
                _httpRequest.Method = new HttpMethod(method);

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain(_ => StringComparer.InvariantCultureIgnoreCase.Equals(_.Key, "Digest"));
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("TRACE")]
            [InlineData("HEAD")]
            [InlineData("DELETE")]
            public async Task WhenMethodHasEmptyBody_DoesNotSetDigestHeader(string method) {
                _httpRequest.Content = new ByteArrayContent(Array.Empty<byte>());
                
                _httpRequest.Method = new HttpMethod(method);

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain(_ => StringComparer.InvariantCultureIgnoreCase.Equals(_.Key, "Digest"));
            }

            [Fact]
            public async Task WhenDigestIsDisabled_DoesNotSetDigestHeader() {
                _settings.DigestHashAlgorithm = new HashAlgorithmName();

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain(_ => StringComparer.InvariantCultureIgnoreCase.Equals(_.Key, "Digest"));
            }

            [Fact]
            public async Task WhenHashAlgorithmIsNotSupported_ThrowsNotSupportedException() {
                _httpRequest.Content = new StringContent("abc123", Encoding.UTF8, "application/json");

                _settings.DigestHashAlgorithm = new HashAlgorithmName("Unsupported");

                Func<Task> act = () => _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                await act.Should().ThrowAsync<NotSupportedException>();
            }

            [Fact]
            public async Task WhenDigestIsAlreadyPresent_DoesNotChangeDigestHeader() {
                _httpRequest.Headers.Add("Digest", "SHA-256=abc123");

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == "Digest" && h.Value.SequenceEqual(new[] {"SHA-256=abc123"}));
            }

            [Fact]
            public async Task WhenDigestIsAlreadyPresent_ButIncorrectlyCased_DoesNotChangeDigestHeader() {
                _httpRequest.Headers.Add("digest", "SHA-256=abc123");

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == "digest" && h.Value.SequenceEqual(new[] {"SHA-256=abc123"}));
                _httpRequest.Headers.Should().NotContainKey("Digest");
            }

            [Fact]
            public async Task WhenRequestHasNoContent_SetsEmptyDigestHeader() {
                _httpRequest.Content = null;

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == "Digest" && h.Value.SequenceEqual(new[] {""}));
            }

            [Fact]
            public async Task ReturnsExpectedString() {
                var bodyBytes = new byte[] {0x01, 0x02, 0x03};
                _httpRequest.Content = new ByteArrayContent(bodyBytes);

                _settings.DigestHashAlgorithm = HashAlgorithmName.SHA512;
                var hashBytes = HashAlgorithm.Create(HashAlgorithmName.SHA512.Name)?.ComputeHash(bodyBytes);

                var base64 = "xyz==";
                A.CallTo(() => _base64Converter.ToBase64(A<byte[]>.That.IsSameSequenceAs(hashBytes)))
                    .Returns(base64);

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == "Digest" && h.Value.SequenceEqual(new[] {"SHA-512=xyz=="}));
            }
        }
    }
}