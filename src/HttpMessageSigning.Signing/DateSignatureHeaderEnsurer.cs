using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class DateSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        public Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            if (signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date) && !request.Headers.Date.HasValue) {
                request.Headers.Date = timeOfSigning;
            }
            
            return Task.CompletedTask;
        }
    }
}