using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Dalion.HttpMessageSigning.Verification.MongoDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services
                .Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; })
                .AddRouting(options => { })
                .AddControllersWithViews().Services
                .AddAuthentication(SignedHttpRequestDefaults.AuthenticationScheme)
                .AddSignedRequests(options => {
                    options.Realm = "Sample web application";
                    options.OnIdentityVerified = (request, successResult) => {
                        var identity = (ClaimsIdentity) successResult.Principal.Identity;
                        Console.WriteLine("Identity '{0}' was authenticated by the request signature.", identity.Name ?? "[NULL]");
                        return Task.CompletedTask;
                    };
                    options.OnIdentityVerificationFailed = (request, failure) => {
                        Console.WriteLine("The request signature could not be verified. Authentication failed: {0}", failure.Failure.Message);
                        return Task.CompletedTask;
                    };
                }).Services

                /* Sample for InMemoryClientStore */
                .AddHttpMessageSignatureVerification(new InMemoryClientStore());

                /* Sample for MongoDbClientStore */
                /*.AddHttpMessageSignatureVerification()
                .AddMongoDbClientStore(provider => new MongoDbClientStoreSettings {
                    ConnectionString = "mongodb://localhost:27017/HttpMessageSigningDb",
                    CollectionName = "known_clients",
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(3)
                })
                .AddMongoDbNonceStore(provider => new MongoDbNonceStoreSettings {
                    ConnectionString = "mongodb://localhost:27017/HttpMessageSigningDb",
                    CollectionName = "client_nonces"
                })*/;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // Register sample client
            var clientStore = app.ApplicationServices.GetRequiredService<IClientStore>();
            clientStore
                .Register(new Client(
                    new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R", HashAlgorithmName.SHA512),
                    TimeSpan.FromMinutes(5),
                    new Claim(SignedHttpRequestClaimTypes.Role, "user.read")))
                .GetAwaiter().GetResult();

            app
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}