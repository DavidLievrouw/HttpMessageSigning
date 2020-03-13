using System.Security.Claims;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services
                .AddRouting(options => { })
                .AddControllersWithViews().Services
                .AddHttpMessageSignatureVerification(new Client(
                    new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R", HashAlgorithmName.SHA512),
                    new Claim(SignedHttpRequestClaimTypes.Role, "users.read")))
                .AddAuthentication(SignedHttpRequestDefaults.AuthenticationScheme)
                .AddSignedRequests(options => {
                    options.Realm = "Sample web application";
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}