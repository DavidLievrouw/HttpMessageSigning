using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample {
    public class Program {
        private static async Task Main(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                var authParam = await SampleSigning(serviceProvider);
                await SampleValidating(serviceProvider, authParam);
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            
            services
                .AddLogging(configure => configure.AddConsole())
                .AddHttpMessageSigning(provider => new SigningSettings {
                    ClientKey = new ClientKey {
                        Id = new KeyId("HttpMessageSigningSample"),
                        Secret = new Secret("yumACY64r%hm")
                    }
                })
                .AddHttpMessageSignatureValidation(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        new KeyId("HttpMessageSigningSample"),
                        new Secret("yumACY64r%hm"),
                        SignatureAlgorithm.HMAC,
                        HashAlgorithm.SHA256,
                        new Claim(Constants.ClaimTypes.Role, "users.read")));
                    return clientStore;
                });
        }

        private static async Task<string> SampleSigning(IServiceProvider serviceProvider) {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            
            var requestSigner = serviceProvider.GetRequiredService<IRequestSigner>();
            await requestSigner.Sign(request);
            
            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                Console.WriteLine("Response:");
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            return request.Headers.Authorization.Parameter;
        }
        
        private static async Task SampleValidating(IServiceProvider serviceProvider, string authParam) {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            request.Headers["Authorization"] = "Signature " + authParam;
            
            var requestSignatureValidator = serviceProvider.GetRequiredService<IRequestSignatureValidator>();
            var validationResult = await requestSignatureValidator.ValidateSignature(request);

            if (validationResult is RequestSignatureValidationResultSuccess successResult) {
                Console.WriteLine("Request signature validation succeeded:");
                var simpleClaims = successResult.ValidatedPrincipal.Claims.Select(c => new {c.Type, Value = c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                Console.WriteLine(claimsString);
                
            } else if (validationResult is RequestSignatureValidationResultFailure failureResult) {
                Console.WriteLine("Request signature validation failed:");
                Console.WriteLine(failureResult.SignatureValidationException);
            }
        }
    }
}