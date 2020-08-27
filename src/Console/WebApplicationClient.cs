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
    public class WebApplicationClient {
        private const string KeyId = "e0e8dcd638334c409e1b88daf821d135";
        private const string Secret = "G#6l$!D16E2UPoYKu&oL@AjAOj9vipKJTSII%*8iY*q6*MOis2R";
        private const int Port = 5000;
        
        public static async Task Run(string[] args) {
            using (var serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider()) {
                using (var signerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>()) {
                    var logger = serviceProvider.GetService<ILogger<WebApplicationClient>>();
                    await SendGetRequest(signerFactory, logger);
                    await SendPostRequest(signerFactory, logger);
                    await SendEncodedRequest(signerFactory, logger);
                    await SendPartiallyEncodedRequest(signerFactory, logger);
                    await SendDecodedRequest(signerFactory, logger);
                    SendConcurrentRequests(signerFactory, logger);
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

        private static async Task SendGetRequest(IRequestSignerFactory requestSignerFactory, ILogger<WebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    logger?.LogInformation("{0} - GET request response: {1}", response.StatusCode, response.StatusCode);
                }
                else {
                    logger?.LogError("{0} - GET request response: {1}", response.StatusCode, response.ReasonPhrase);
                }
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static async Task SendPostRequest(IRequestSignerFactory requestSignerFactory, ILogger<WebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo"),
                Content = new StringContent("{\"id\": 42 }", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    logger?.LogInformation("{0} - POST request response: {1}", response.StatusCode, response.StatusCode);
                }
                else {
                    logger?.LogError("{0} - POST request response: {1}", response.StatusCode, response.ReasonPhrase);
                }
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static async Task SendEncodedRequest(IRequestSignerFactory requestSignerFactory, ILogger<WebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo/api/%7BBrooks%7D%20was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query%2Bstring=%7Bbrooks%7D")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    logger?.LogInformation("{0} - Encoded GET request response: {1}", response.StatusCode, response.StatusCode);
                }
                else {
                    logger?.LogError("{0} - Encoded GET request response: {1}", response.StatusCode, response.ReasonPhrase);
                }
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static async Task SendPartiallyEncodedRequest(IRequestSignerFactory requestSignerFactory, ILogger<WebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo/api/{Brooks} was%20here/api/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7Bbrooks}")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    logger?.LogInformation("{0} - Encoded GET request response: {1}", response.StatusCode, response.StatusCode);
                }
                else {
                    logger?.LogError("{0} - Encoded GET request response: {1}", response.StatusCode, response.ReasonPhrase);
                }
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static async Task SendDecodedRequest(IRequestSignerFactory requestSignerFactory, ILogger<WebApplicationClient> logger) {
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:" + Port + "/userinfo/api/{Brooks} was here/api/David & Partners + Siebe at 100% * co.?query+string={brooks}")
            };

            var requestSigner = requestSignerFactory.CreateFor(KeyId);
            await requestSigner.Sign(request);

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    logger?.LogInformation("{0} - Encoded GET request response: {1}", response.StatusCode, response.StatusCode);
                }
                else {
                    logger?.LogError("{0} - Encoded GET request response: {1}", response.StatusCode, response.ReasonPhrase);
                }
                var responseContentTask = response.Content?.ReadAsStringAsync();
                var responseContent = responseContentTask == null ? null : await responseContentTask;
                if (responseContent != null) logger?.LogInformation(responseContent);
            }
        }
        
        private static void SendConcurrentRequests(IRequestSignerFactory requestSignerFactory, ILogger<WebApplicationClient> logger) {
            for (var i = 0; i < 199; i++) {
                Task.Factory.StartNew(() => SendGetRequest(requestSignerFactory, logger), TaskCreationOptions.LongRunning);
            }
        }
    }
}