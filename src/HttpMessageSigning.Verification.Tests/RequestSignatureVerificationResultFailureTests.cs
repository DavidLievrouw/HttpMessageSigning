using System;
using System.Net.Http;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class RequestSignatureVerificationResultFailureTests {
        private readonly Client _client;
        private readonly SignatureVerificationFailure _failure;
        private readonly HttpRequestForVerification _request;
        private readonly RequestSignatureVerificationResultFailure _sut;

        public RequestSignatureVerificationResultFailureTests() {
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
            _failure = new InvalidSignatureStringSignatureVerificationFailure(
                "The verification failed",
                new InvalidOperationException("Epic fail"));
            _sut = new RequestSignatureVerificationResultFailure(_client, _request, _failure);
        }

        public class Construction : RequestSignatureVerificationResultFailureTests {
            [Fact]
            public void AllowsForNullClient() {
                RequestSignatureVerificationResultFailure actual = null;
                Action act = () => actual = new RequestSignatureVerificationResultFailure(null, _request, _failure);

                act.Should().NotThrow();
                actual.Client.Should().BeNull();
            }

            [Fact]
            public void AllowsForNullRequest() {
                RequestSignatureVerificationResultFailure actual = null;
                Action act = () => actual = new RequestSignatureVerificationResultFailure(_client, null, _failure);

                act.Should().NotThrow();
                actual.RequestForVerification.Should().BeNull();
            }

            [Fact]
            public void GivenNullFailure_ThrowsArgumentNullException() {
                Action act = () => new RequestSignatureVerificationResultFailure(_client, _request, null);

                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void AssignsPropertyValues() {
                _sut.Client.Should().Be(_client);
                _sut.RequestForVerification.Should().Be(_request);
                _sut.Failure.Should().Be(_failure);
            }
        }

        public class IsSuccess : RequestSignatureVerificationResultFailureTests {
            [Fact]
            public void ReturnsFalse() {
                _sut.IsSuccess.Should().BeFalse();
            }
        }
    }
}