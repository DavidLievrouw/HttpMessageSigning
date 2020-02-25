using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample {
    public class Program {
        private static async Task Main(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
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
                });
        }
    }
}