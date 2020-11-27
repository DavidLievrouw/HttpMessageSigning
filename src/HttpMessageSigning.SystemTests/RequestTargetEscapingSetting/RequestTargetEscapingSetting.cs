#if NETCORE
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Dalion.HttpMessageSigning.RequestTargetEscapingSetting {
    public class RequestTargetEscapingSetting : IDisposable {
        private readonly ITestOutputHelper _output;
        private readonly SignedRequestAuthenticationOptions _authenticationOptions;
        private ServiceProvider _serviceProvider;
        private IRequestSignerFactory _requestSignerFactory;
        private IRequestSignatureVerifier _verifier;

        public RequestTargetEscapingSetting(ITestOutputHelper output) {
            _output = output;
            _authenticationOptions = new SignedRequestAuthenticationOptions();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
            _requestSignerFactory?.Dispose();
            _verifier?.Dispose();
        }

        [Theory]
        [InlineData(RequestTargetEscaping.Unescaped)]
        [InlineData(RequestTargetEscaping.RFC3986)]
        [InlineData(RequestTargetEscaping.RFC2396)]
        [InlineData(RequestTargetEscaping.OriginalString)]
        public async Task WhenRequestTargetEscapingSettingIsAMatch_VerificationSucceeds(RequestTargetEscaping escaping) {
            _serviceProvider = new ServiceCollection().Configure(services => ConfigureServices(services, escaping, escaping)).BuildServiceProvider();
            _requestSignerFactory = _serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _verifier = _serviceProvider.GetRequiredService<IRequestSignatureVerifier>();

            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult.IsSuccess.Should().BeTrue();
            if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }

        [Fact]
        public async Task WhenRequestTargetEscapingSettingIsAMismatch_VerificationFails() {
            _serviceProvider = new ServiceCollection()
                .Configure(services => ConfigureServices(services, RequestTargetEscaping.RFC3986, RequestTargetEscaping.Unescaped))
                .BuildServiceProvider();
            _requestSignerFactory = _serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _verifier = _serviceProvider.GetRequiredService<IRequestSignatureVerifier>();

            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://dalion.eu/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7BBrooks%7D"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult.IsSuccess.Should().BeFalse();
            if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
            }
        }

        private static void ConfigureServices(IServiceCollection services, RequestTargetEscaping escapingForSigning, RequestTargetEscaping escapingForVerification) {
            services
                .AddHttpMessageSigning()
                .UseKeyId("e0e8dcd638334c409e1b88daf821d135")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseDigestAlgorithm(HashAlgorithmName.SHA256)
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .UseRequestTargetEscaping(escapingForSigning)
                .Services
                .AddHttpMessageSignatureVerification()
                .UseAspNetCoreSignatureVerification()
                .UseClient(Client.Create(
                    "e0e8dcd638334c409e1b88daf821d135",
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                    options => {
                        options.RequestTargetEscaping = escapingForVerification;
                        options.Claims = new[] {
                            new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                        };
                    }));
        }
    }
}
#endif