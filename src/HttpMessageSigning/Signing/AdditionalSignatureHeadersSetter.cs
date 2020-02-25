using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class AdditionalSignatureHeadersSetter : IAdditionalSignatureHeadersSetter {
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;

        public AdditionalSignatureHeadersSetter(ISignatureHeaderEnsurer dateHeaderEnsurer, ISignatureHeaderEnsurer digestHeaderEnsurer) {
            _dateHeaderEnsurer = dateHeaderEnsurer ?? throw new ArgumentNullException(nameof(dateHeaderEnsurer));
            _digestHeaderEnsurer = digestHeaderEnsurer ?? throw new ArgumentNullException(nameof(digestHeaderEnsurer));
        }

        public async Task AddMissingRequiredHeadersForSignature(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            
            signingSettings.Validate();
            
            await _dateHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
            await _digestHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
        }
    }
}