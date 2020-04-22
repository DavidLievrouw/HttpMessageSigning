using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class CreatedSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        public Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            if (signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Created) && !request.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                request.Headers.Add(HeaderName.PredefinedHeaderNames.Created, timeOfSigning.ToUnixTimeSeconds().ToString());
            }
            
            return Task.CompletedTask;
        }
    }
}