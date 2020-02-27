using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Internal;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        internal static async Task<HttpRequestForSigning> ToRequestForSigning(this HttpRequest request, ISignatureAlgorithm signatureAlgorithm, Signature signature) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            
            if (request == null) return null;

            var requestMessage = new HttpRequestForSigning {
                SignatureAlgorithmName = signatureAlgorithm.Name,
                RequestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute),
                Method = string.IsNullOrEmpty(request.Method)
                    ? HttpMethod.Get
                    : new HttpMethod(request.Method)
            };

            foreach (var header in request.Headers) {
                requestMessage.Headers[header.Key] = header.Value;
            }

            if (ShouldReadBody(request, signature) && request.Body != null) {
                request.EnableRewind();

                using (var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true)) {
                    requestMessage.Body = await reader.ReadToEndAsync();
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
            }

            return requestMessage;
        }

        private static bool ShouldReadBody(HttpRequest request, Signature signature) {
            if (request.Body == null) return false;
            return (signature.Headers?.Contains(HeaderName.PredefinedHeaderNames.Digest) ?? false) || 
                   (request.Headers?.ContainsKey(HeaderName.PredefinedHeaderNames.Digest) ?? false);
        }
    }
}