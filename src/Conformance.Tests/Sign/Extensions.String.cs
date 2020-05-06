using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.HttpMessages;

namespace Dalion.HttpMessageSigning.Sign {
    public static class Extensions {
        public static async Task<HttpRequestMessage> ToHttpRequestMessage(this Task<string> messageString) {
            if (messageString == null) return null;
            return HttpRequestMessageParser.Parse(await messageString);
        }
        
        public static HttpRequestMessage ToHttpRequestMessage(this string messageString) {
            if (messageString == null) return null;
            return HttpRequestMessageParser.Parse(messageString);
        }
    }
}