using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Console {
    public class SampleHMACClient {
        private const string KeyId = "e0e8dcd638334c409e1b88daf821d135";
        private const string Secret = "G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R";

        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                    var logger = serviceProvider.GetService<ILogger<SampleHMACClient>>();
                    await Send(signerFactory, logger);
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services) {
            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddHttpMessageSigning(
                    new KeyId(KeyId),
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning(Secret, HashAlgorithmName.SHA512),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256
                    });
        }

        private static async Task Send(IRequestSignerFactory requestSignerFactory, ILogger<SampleHMACClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5000/userinfo")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                logger?.LogInformation("Response: " + response.StatusCode);
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
    }
}