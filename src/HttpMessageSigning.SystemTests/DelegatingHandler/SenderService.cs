using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.DelegatingHandler {
    internal class SenderService {
        private readonly HttpClient _httpClient;
        
        public SenderService(HttpClient httpClient) {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<HttpResponseMessage> SendTo(Uri uri) {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            
            var request = new HttpRequestMessage {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            return _httpClient.SendAsync(request);
        }
    }
}