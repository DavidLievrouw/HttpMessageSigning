using System;
using System.Security.Claims;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OwinApplication {
    public static class IocStartup {
        public static IServiceProvider BuildServiceProvider() {
            var services = new ServiceCollection();

            services
                .AddLogging(builder => {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Debug);
                })
                .AddHttpMessageSignatureVerification(
                    new Client(
                        new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                        "HttpMessageSigningSampleHMAC",
                        SignatureAlgorithm.CreateForVerification("G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R", HashAlgorithmName.SHA512),
                        TimeSpan.FromMinutes(5),
                        new Claim(SignedHttpRequestClaimTypes.Role, "user.read")));

            return services.BuildServiceProvider();
        }
    }
}