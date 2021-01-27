using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Dalion.HttpMessageSigning.Verification.FileSystem;
using Dalion.HttpMessageSigning.Verification.MongoDb;
using Dalion.HttpMessageSigning.Verification.SqlServer;
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

                /* Sample for InMemoryClientStore and InMemoryNonceStore */
                .AddHttpMessageSignatureVerification()
                .UseAspNetCoreSignatureVerification()
                .UseClient(
                    Client.Create(
                        new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                        "Sample client",
                        SignatureAlgorithm.CreateForVerification("G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R"),
                        options => options.Claims = new[] {new Claim(SignedHttpRequestClaimTypes.Role, "user.read")})
                )

                /* Sample for storing Clients and Nonces in MongoDB instead of in-memory */
                /*.UseMongoDbClientStore(provider => new MongoDbClientStoreSettings {
                    ConnectionString = "mongodb://localhost:27017/HttpMessageSigningDb",
                    CollectionName = "known_clients",
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(3),
                    SharedSecretEncryptionKey = "The_Big_S3cr37"
                })
                .UseMongoDbNonceStore(provider => new MongoDbNonceStoreSettings {
                    ConnectionString = "mongodb://localhost:27017/HttpMessageSigningDb",
                    CollectionName = "client_nonces"
                })*/
                
                /* Sample for storing Clients and Nonces in SqlServer instead of in-memory */
                /*.UseSqlServerClientStore(provider => new SqlServerClientStoreSettings {
                    ConnectionString = "Server=.;Database=HttpMessageSigning;User Id=dalion;Password=Dalion123;",
                    SharedSecretEncryptionKey = "The_Big_S3cr37",
                    ClientsTableName = "dbo.Clients",
                    ClientClaimsTableName = "dbo.ClientClaims",
                    MigrationsTableName = "dbo.ClientVersions",
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(3)
                })
                .UseSqlServerNonceStore(provider => new SqlServerNonceStoreSettings {
                    ConnectionString = "Server=.;Database=HttpMessageSigning;User Id=dalion;Password=Dalion123;",
                    NonceTableName = "dbo.Nonces",
                    MigrationsTableName = "dbo.NonceVersions"
                })*/
                
                /* Sample for storing Clients and Nonces on FileSystem instead of in-memory */
                .UseFileSystemClientStore(provider => new FileSystemClientStoreSettings {
                    FilePath = Path.Combine(Path.GetTempPath(), "Clients.xml"),
                    SharedSecretEncryptionKey = "The_Big_S3cr37",
                    ClientCacheEntryExpiration = TimeSpan.FromMinutes(3)
                })
                .UseFileSystemNonceStore(provider => new FileSystemNonceStoreSettings {
                    FilePath = Path.Combine(Path.GetTempPath(), "Nonces.xml")
                })
                
                .UseClaimsPrincipalFactory<CustomClaimsPrincipalFactory>().Services
                .AddSingleton<CustomClaimsPrincipalFactory>()
                .AddHttpContextAccessor();
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