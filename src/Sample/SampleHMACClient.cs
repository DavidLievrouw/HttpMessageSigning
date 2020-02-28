using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample {
    public class SampleHMACClient {
        private const string KeyId = "Dashboard";
        private const string Secret = "G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R";

        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                    await Send(signerFactory);
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            services
                .AddLogging(configure => configure.AddConsole())
                .AddHttpMessageSigning(
                    new KeyId(KeyId),
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning(Secret, HashAlgorithmName.SHA512),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256
                    });
        }

        private static async Task Send(IRequestSignerFactory requestSignerFactory) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:54004/api/userinfo")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                Console.WriteLine("Response: " + response.StatusCode);
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) Console.WriteLine(responseContent);
            }
        }
    }
}