using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class CompositeSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;

        public CompositeSignatureHeaderEnsurer(
            ISignatureHeaderEnsurer dateHeaderEnsurer, 
            ISignatureHeaderEnsurer digestHeaderEnsurer) {
            _dateHeaderEnsurer = dateHeaderEnsurer ?? throw new ArgumentNullException(nameof(dateHeaderEnsurer));
            _digestHeaderEnsurer = digestHeaderEnsurer ?? throw new ArgumentNullException(nameof(digestHeaderEnsurer));
        }

        public async Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            await _dateHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
            await _digestHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
        }
    }
}