using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class CompositeSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _createdHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _expiresHeaderEnsurer;

        public CompositeSignatureHeaderEnsurer(
            ISignatureHeaderEnsurer dateHeaderEnsurer, 
            ISignatureHeaderEnsurer digestHeaderEnsurer,
            ISignatureHeaderEnsurer createdHeaderEnsurer,
            ISignatureHeaderEnsurer expiresHeaderEnsurer) {
            _dateHeaderEnsurer = dateHeaderEnsurer ?? throw new ArgumentNullException(nameof(dateHeaderEnsurer));
            _digestHeaderEnsurer = digestHeaderEnsurer ?? throw new ArgumentNullException(nameof(digestHeaderEnsurer));
            _createdHeaderEnsurer = createdHeaderEnsurer ?? throw new ArgumentNullException(nameof(createdHeaderEnsurer));
            _expiresHeaderEnsurer = expiresHeaderEnsurer ?? throw new ArgumentNullException(nameof(expiresHeaderEnsurer));
        }

        public async Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            await _dateHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
            await _createdHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
            await _expiresHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
            await _digestHeaderEnsurer.EnsureHeader(request, signingSettings, timeOfSigning);
        }
    }
}