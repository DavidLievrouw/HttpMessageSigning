using System.Security.Claims;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OwinApplication {
    public static class Extensions {
        public static IServiceCollection ConfigureServices(this IServiceCollection services) {
            services
                .AddLogging(builder => {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Debug);
                })
                .AddHttpMessageSignatureVerification()
                .UseOwinSignatureVerification()
                .UseClient(
                    Client.Create(
                        new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                        "HttpMessageSigningSampleHMAC",
                        SignatureAlgorithm.CreateForVerification("G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R", HashAlgorithmName.SHA512),
                        options => {
                            options.Claims = new[] {
                                new Claim(SignedHttpRequestClaimTypes.Role, "user.read")
                            };
                        })
                );

            return services;
        }
    }
}