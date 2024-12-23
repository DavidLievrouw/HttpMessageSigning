﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
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
    public class SampleRSA {
        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                    using (var verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>()) {
                        var logger = serviceProvider.GetService<ILogger<SampleRSA>>();
                        var signedRequestForRSA = await SampleSignRSA(signerFactory);
                        await SampleVerify(verifier, signedRequestForRSA, logger);
                    }
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            var cert = new X509Certificate2(File.ReadAllBytes("./dalion.local.pfx"), "CertP@ss123", X509KeyStorageFlags.Exportable);

            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddHttpMessageSigning()
                .UseKeyId((KeyId)"4d8f14b6c4184dc1b677c88a2b60bfd2")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning(cert))
                .Services
                .AddHttpMessageSignatureVerification()
                .UseClient(Client.Create(
                    (KeyId)"4d8f14b6c4184dc1b677c88a2b60bfd2",
                    "HttpMessageSigningSampleRSA",
                    SignatureAlgorithm.CreateForVerification(cert),
                    options => options.Claims = new[] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ));
        }

        private static async Task<HttpRequestMessage> SampleSignRSA(IRequestSignerFactory requestSignerFactory) {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            var requestSigner = requestSignerFactory.CreateFor((KeyId)"4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            return request;
        }

        private static async Task SampleVerify(IRequestSignatureVerifier verifier, HttpRequestMessage clientRequest, ILogger<SampleRSA> logger) {
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