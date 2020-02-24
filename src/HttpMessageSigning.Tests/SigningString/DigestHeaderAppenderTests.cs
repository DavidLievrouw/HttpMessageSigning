using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class DigestHeaderAppenderTests {
        private readonly IBase64Converter _base64Converter;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly IHashAlgorithmFactory _hashAlgorithmFactory;
        private readonly HttpRequestMessage _httpRequest;
        private readonly DigestHeaderAppender _sut;

        public DigestHeaderAppenderTests() {
            _hashAlgorithm = HashAlgorithm.SHA384;
            _httpRequest = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://dalion.eu/api/resource/id1")
            };
            FakeFactory.Create(out _base64Converter, out _hashAlgorithmFactory);
            _sut = new DigestHeaderAppender(_httpRequest, _hashAlgorithm, _base64Converter, _hashAlgorithmFactory);
        }

        public class BuildStringToAppend : DigestHeaderAppenderTests {
            [Fact]
            public void WhenMethodIsGet_ReturnsEmptyString() {
                _httpRequest.Method = HttpMethod.Get;

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Digest);

                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void WhenRequestHasNoContent_ReturnsEmptyDigestValue() {
                _httpRequest.Content = null;

                var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Digest);

                actual.Should().Be("\ndigest: ");
            }

            [Fact]
            public void ReturnsExpectedString() {
                _httpRequest.Content = new StringContent("abc123", Encoding.UTF8, MediaTypeNames.Application.Json);

                using (var hashAlgorithm = A.Fake<IHashAlgorithm>()) {
                    A.CallTo(() => _hashAlgorithmFactory.Create(_hashAlgorithm))
                        .Returns(hashAlgorithm);

                    var hashBytes = new byte[] {0x01, 0x02};
                    A.CallTo(() => hashAlgorithm.ComputeHash("abc123"))
                        .Returns(hashBytes);

                    A.CallTo(() => hashAlgorithm.Name)
                        .Returns("SHA-384");

                    var base64 = "xyz==";
                    A.CallTo(() => _base64Converter.ToBase64(hashBytes))
                        .Returns(base64);

                    var actual = _sut.BuildStringToAppend(HeaderName.PredefinedHeaderNames.Digest);

                    actual.Should().Be("\ndigest: SHA-384=xyz==");
                }
            }
        }
    }
}