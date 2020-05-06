using System;
using System.Net.Http;
using System.Text;

namespace Dalion.HttpMessageSigning.HttpMessages {
    public static class HttpMessageGenerator {
        public static HttpRequestMessage GenerateMessage(string messageName, DateTimeOffset date) {
            var sb = new StringBuilder();
            sb.Append(HttpMessageReader.Read(messageName));
            sb.Append("Date: " + date.ToString("R"));
            sb.Append("\n\n");
            sb.Append("{\"hello\": \"world\"}");

            return HttpRequestMessageParser.Parse(sb.ToString());
        }
    }
}