using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample {
    public class SampleHMAC {
        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
                var verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>();

                var signedRequestForHMAC = await SampleSignHMAC(signerFactory);
                await SampleVerify(verifier, signedRequestForHMAC);
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            services
                .AddLogging(configure => configure.AddConsole())
                .AddHttpMessageSigning(
                    new KeyId("HttpMessageSigningSampleHMAC"),
                    provider => new SigningSettings {
                        SignatureAlgorithm = new HMACSignatureAlgorithm("yumACY64r%hm", HashAlgorithmName.SHA256),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256,
                        Expires = TimeSpan.FromMinutes(1),
                        Headers = new [] {
                            (HeaderName)"Dalion-App-Id"
                        }
                    })
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        new KeyId("HttpMessageSigningSampleHMAC"),
                        new HMACSignatureAlgorithm("yumACY64r%hm", HashAlgorithmName.SHA256),
                        new Claim(Constants.ClaimTypes.Role, "users.read")));
                    return clientStore;
                });
        }

        private static async Task<HttpRequestMessage> SampleSignHMAC(IRequestSignerFactory requestSignerFactory) {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            var requestSigner = requestSignerFactory.CreateFor("HttpMessageSigningSampleHMAC");
            await requestSigner.Sign(request);

            return request;
        }

        private static async Task SampleVerify(IRequestSignatureVerifier verifier, HttpRequestMessage clientRequest) {
            var receivedRequest = CreateFakeReceivedServerSideHttpRequest(clientRequest);

            var verificationResult = await verifier.VerifySignature(receivedRequest);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                Console.WriteLine("Request signature verification succeeded:");
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                Console.WriteLine(claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                Console.WriteLine("Request signature verification failed:");
                Console.WriteLine(failureResult.SignatureVerificationException);
            }
        }

        private static DefaultHttpRequest CreateFakeReceivedServerSideHttpRequest(HttpRequestMessage clientRequest) {
            var request = new DefaultHttpRequest(new DefaultHttpContext()) {
                Method = "POST",
                Scheme = "https",
                Host = new HostString("httpbin.org", 443),
                Path = new PathString("/post"),
                Headers = {
                    {"Authorization", clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter}
                }
            };
            if (clientRequest.Headers.Contains("Date")) {
                request.Headers.Add("Date", clientRequest.Headers.GetValues("Date").ToArray());
            }

            if (clientRequest.Headers.Contains("Dalion-App-Id")) {
                request.Headers.Add("Dalion-App-Id", clientRequest.Headers.GetValues("Dalion-App-Id").ToArray());
            }

            if (clientRequest.Headers.Contains("Digest")) {
                request.Headers.Add("Digest", clientRequest.Headers.GetValues("Digest").ToArray());
            }

            return request;
        }
    }
}