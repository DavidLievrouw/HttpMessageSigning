using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
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
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.EnsureHeader(null, _settings, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.EnsureHeader(_httpRequest, null, _timeOfSigning);
                act.Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("TRACE")]
            [InlineData("HEAD")]
            [InlineData("DELETE")]
            public async Task WhenMethodDoesNotHaveBody_DoesNotSetDigestHeader(string method) {
                _httpRequest.Method = new HttpMethod(method);

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain("Digest");
            }

            [Fact]
            public async Task WhenDigestIsDisabled_DoesNotSetDigestHeader() {
                _settings.DigestHashAlgorithm = new HashAlgorithmName();

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain("Digest");
            }
            
            [Fact]
            public void WhenHashAlgorithmIsNotSupported_ThrowsNotSupportedException() {
                _httpRequest.Content = new StringContent("abc123", Encoding.UTF8, MediaTypeNames.Application.Json);
                
                _settings.DigestHashAlgorithm = new HashAlgorithmName("Unsupported");

                Func<Task> act = () =>  _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                act.Should().Throw<NotSupportedException>();
            }

            [Fact]
            public async Task WhenDigestIsAlreadyPresent_DoesNotChangeDigestHeader() {
                _httpRequest.Headers.Add("Digest", "SHA-256=abc123");

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == "Digest" && h.Value == new Microsoft.Extensions.Primitives.StringValues("SHA-256=abc123"));
            }
            
            [Fact]
            public async Task WhenDigestIsAlreadyPresent_ButIncorrectlyCased_DoesNotChangeDigestHeader() {
                _httpRequest.Headers.Add("digest", "SHA-256=abc123");

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().Contain(h => h.Key == "digest" && h.Value == new Microsoft.Extensions.Primitives.StringValues("SHA-256=abc123"));
                _httpRequest.Headers.Should().NotContain("Digest");
            }

            [Fact]
            public async Task WhenRequestHasNoContent_SetsEmptyDigestHeader() {
                _httpRequest.Content = null;

                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);

                _httpRequest.Headers.Should().NotContain("Digest");
            }

            [Fact]
            public async Task ReturnsExpectedString() {
                _httpRequest.Content = new StringContent("abc123", Encoding.UTF8, MediaTypeNames.Application.Json);
                var expectedBodyBytes = Encoding.UTF8.GetBytes("abc123");
                
                _settings.DigestHashAlgorithm = HashAlgorithmName.SHA512;
                var hashBytes = HashAlgorithm.Create(HashAlgorithmName.SHA512.Name)?.ComputeHash(expectedBodyBytes);

                var base64 = "xyz==";
                A.CallTo(() => _base64Converter.ToBase64(A<byte[]>.That.IsSameSequenceAs(hashBytes)))
                    .Returns(base64);
                
                await _sut.EnsureHeader(_httpRequest, _settings, _timeOfSigning);
                
                _httpRequest.Headers.Should().Contain(h => h.Key == "Digest" && h.Value == new Microsoft.Extensions.Primitives.StringValues("SHA-512=xyz=="));
            }
        }
    }
}