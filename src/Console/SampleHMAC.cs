﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Console {
    public class SampleHMAC {
        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                    using (var verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>()) {
                        var logger = serviceProvider.GetService<ILogger<SampleHMAC>>();
                        var signedRequestForHMAC = await SampleSignHMAC(signerFactory);
                        await SampleVerify(verifier, signedRequestForHMAC, logger);
                    }
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddHttpMessageSigning()
                .UseKeyId("e0e8dcd638334c409e1b88daf821d135")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseDigestAlgorithm(HashAlgorithmName.SHA256)
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .Services
                .AddHttpMessageSignatureVerification()
                .UseClient(Client.Create(
                    "e0e8dcd638334c409e1b88daf821d135",
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                    options => options.Claims = new [] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ));
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

            var requestSigner = requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);
            
            return request;
        }

        private static async Task SampleVerify(IRequestSignatureVerifier verifier, HttpRequestMessage clientRequest, ILogger<SampleHMAC> logger) {
            var receivedRequest = await clientRequest.ToServerSideHttpRequest();

            var verificationResult = await verifier.VerifySignature(receivedRequest, new SignedRequestAuthenticationOptions());
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                logger?.LogInformation("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                logger?.LogWarning("Request signature verification failed: {0}", failureResult.Failure);
            }
        }
    }
}