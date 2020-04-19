using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Console {
    public class AspNetCoreWebApplicationClient {
        private const string KeyId = "e0e8dcd638334c409e1b88daf821d135";
        private const string Secret = "G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R";
        private const int Port = 5000;
        
        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                    var logger = serviceProvider.GetService<ILogger<AspNetCoreWebApplicationClient>>();
                    await SendGetRequest(signerFactory, logger);
                    await SendPostRequest(signerFactory, logger);
                    await SendEncodedRequest(signerFactory, logger);
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

        private static async Task SendGetRequest(IRequestSignerFactory requestSignerFactory, ILogger<AspNetCoreWebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                logger?.LogInformation("GET request response: " + response.StatusCode);
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static async Task SendPostRequest(IRequestSignerFactory requestSignerFactory, ILogger<AspNetCoreWebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo"),
                Content = new StringContent("{\"id\": 42 }", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                logger?.LogInformation("POST request response: " + response.StatusCode);
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static async Task SendEncodedRequest(IRequestSignerFactory requestSignerFactory, ILogger<AspNetCoreWebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo/api/%7BBrooks%7D%20was%20here/api/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                logger?.LogInformation("Encoded request response: " + response.StatusCode);
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
    }
}