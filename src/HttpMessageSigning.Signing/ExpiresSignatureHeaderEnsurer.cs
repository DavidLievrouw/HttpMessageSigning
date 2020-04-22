using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class ExpiresSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        public Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            
            if (signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires) && !request.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                request.Headers.Add(HeaderName.PredefinedHeaderNames.Expires, timeOfSigning.Add(signingSettings.Expires).ToUnixTimeSeconds().ToString());
            }
            
            return Task.CompletedTask;
        }
    }
}