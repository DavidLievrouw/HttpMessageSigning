using System;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class RequestSignatureVerificationResultSuccessTests {
        private readonly Client _client;
        private readonly ClaimsPrincipal _principal;
        private readonly HttpRequestForVerification _request;
        private readonly RequestSignatureVerificationResultSuccess _sut;

        public RequestSignatureVerificationResultSuccessTests() {
            _request = new HttpRequestForVerification {
                Method = HttpMethod.Post,
                RequestUri = "https://unittest.com:9000",
                Signature = (Signature) TestModels.Signature.Clone()
            };
            _client = new Client(
                _request.Signature.KeyId,
                "Unit test app",
                new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(1));
            _principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
            _sut = new RequestSignatureVerificationResultSuccess(_client, _request, _principal);
        }

        public class Construction : RequestSignatureVerificationResultSuccessTests {
            [Fact]
            public void GivenNullClient_ThrowsArgumentNullException() {
                Action act = () => new RequestSignatureVerificationResultSuccess(null, _request, _principal);

                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => new RequestSignatureVerificationResultSuccess(_client, null, _principal);

                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullFailure_ThrowsArgumentNullException() {
                Action act = () => new RequestSignatureVerificationResultSuccess(_client, _request, null);

                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void AssignsPropertyValues() {
                _sut.Client.Should().Be(_client);
                _sut.RequestForVerification.Should().Be(_request);
                _sut.Principal.Should().Be(_principal);
            }
        }

        public class IsSuccess : RequestSignatureVerificationResultSuccessTests {
            [Fact]
            public void ReturnsTrue() {
                _sut.IsSuccess.Should().BeTrue();
            }
        }
    }
}