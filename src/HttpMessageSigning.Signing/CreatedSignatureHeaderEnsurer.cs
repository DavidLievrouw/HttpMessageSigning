using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class CreatedSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        public Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            // Cannot send headers with braces in .NET
            // See https://stackoverflow.com/a/51039555
            var sanitizedHttpHeaderName = HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName();
            
            if (signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Created) && !request.Headers.Contains(sanitizedHttpHeaderName)) {
                request.Headers.Add(sanitizedHttpHeaderName, timeOfSigning.ToUnixTimeSeconds().ToString());
            }
            
            return Task.CompletedTask;
        }
    }
}