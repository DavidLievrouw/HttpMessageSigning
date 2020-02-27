using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class SignatureVerifierTests {
        private readonly IVerificationTask _allHeadersPresentVerificationTask;
        private readonly IVerificationTask _createdHeaderGuardVerificationTask;
        private readonly IVerificationTask _creationTimeVerificationTask;
        private readonly IVerificationTask _expirationTimeVerificationTask;
        private readonly IVerificationTask _expiresHeaderGuardVerificationTask;
        private readonly IVerificationTask _digestVerificationTask;
        private readonly IVerificationTask _knownAlgorithmVerificationTask;
        private readonly IVerificationTask _matchingAlgorithmVerificationTask;
        private readonly IVerificationTask _matchingSignatureStringVerificationTask;
        private readonly SignatureVerifier _sut;

        public SignatureVerifierTests() {
            FakeFactory.Create(
                out _knownAlgorithmVerificationTask,
                out _matchingAlgorithmVerificationTask,
                out _creationTimeVerificationTask,
                out _createdHeaderGuardVerificationTask,
                out _expiresHeaderGuardVerificationTask,
                out _allHeadersPresentVerificationTask,
                out _expirationTimeVerificationTask,
                out _digestVerificationTask,
                out _matchingSignatureStringVerificationTask);
            _sut = new SignatureVerifier(
                _knownAlgorithmVerificationTask,
                _matchingAlgorithmVerificationTask,
                _createdHeaderGuardVerificationTask,
                _expiresHeaderGuardVerificationTask,
                _allHeadersPresentVerificationTask,
                _creationTimeVerificationTask,
                _expirationTimeVerificationTask,
                _digestVerificationTask,
                _matchingSignatureStringVerificationTask);
        }

        public class VerifySignature : SignatureVerifierTests {
            private readonly Signature _signature;
            private readonly Client _client;
            private readonly HttpRequestForSigning _signedRequest;

            public VerifySignature() {
                _signature = new Signature {KeyId = "client1"};
                
                _signedRequest = new HttpRequestForSigning {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://unittest.com:9001")
                };
                
                _client = new Client("client1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                
                A.CallTo(() => _knownAlgorithmVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _matchingAlgorithmVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _createdHeaderGuardVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _expiresHeaderGuardVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _allHeadersPresentVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _creationTimeVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _expirationTimeVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _digestVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
                A.CallTo(() => _matchingSignatureStringVerificationTask.Verify(A<HttpRequestForSigning>._, A<Signature>._, A<Client>._)).Returns((Exception)null);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(null, _signature, _client);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSignature_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.VerifySignature(_signedRequest, null, _client);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullClient_ThrowsArgumentException() {
                Func<Task> act = () => _sut.VerifySignature(_signedRequest, _signature, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task VerifiesThatAlgorithmIsSupported() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _knownAlgorithmVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatTheClientAlgorithmMatchesTheSignature() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _matchingAlgorithmVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatTheCreatedHeaderIsNotIncludedInTheSignatureForCertainSignatureAlgorithms() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _createdHeaderGuardVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatTheExpiresHeaderIsNotIncludedInTheSignatureForCertainSignatureAlgorithms() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _expiresHeaderGuardVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatAllHeadersInTheSignatureArePresentInTheRequest() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _allHeadersPresentVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatTheCreatedHeaderValueIsInThePast() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _creationTimeVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatTheSignatureIsNotExpired() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _expirationTimeVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesTheDigestHeader() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _digestVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task VerifiesThatTheSignatureStringMatches() {
                await _sut.VerifySignature(_signedRequest, _signature, _client);
                A.CallTo(() => _matchingSignatureStringVerificationTask.Verify(_signedRequest, _signature, _client)).MustHaveHappened();
            }

            [Fact]
            public async Task WhenNoVerificationTaskFails_ReturnsNull() {
                var actual = await _sut.VerifySignature(_signedRequest, _signature, _client);
                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenAVerificationTaskFails_ReturnsFirstFailure_DoesNotRunSubsequentTasks() {
                var firstFailure = new SignatureVerificationException("Invalid");
                A.CallTo(() => _matchingAlgorithmVerificationTask.Verify(_signedRequest, _signature, _client))
                    .Returns(firstFailure);

                var secondFailure = new SignatureVerificationException("Invalid");
                A.CallTo(() => _createdHeaderGuardVerificationTask.Verify(_signedRequest, _signature, _client))
                    .Returns(secondFailure);

                var actual = await _sut.VerifySignature(_signedRequest, _signature, _client);

                actual.Should().Be(firstFailure);
            }
        }
    }
}