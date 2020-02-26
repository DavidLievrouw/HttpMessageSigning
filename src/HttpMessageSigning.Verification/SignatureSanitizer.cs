using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureSanitizer : ISignatureSanitizer {
        private readonly IDefaultSignatureHeadersProvider _defaultSignatureHeadersProvider;
        
        public SignatureSanitizer(IDefaultSignatureHeadersProvider defaultSignatureHeadersProvider) {
            _defaultSignatureHeadersProvider = defaultSignatureHeadersProvider ?? throw new ArgumentNullException(nameof(defaultSignatureHeadersProvider));
        }

        public Task<Signature> Sanitize(Signature signature, Client client) {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var newSignature = (Signature)signature.Clone();
            
            if (newSignature.Headers == null || !newSignature.Headers.Any()) {
                newSignature.Headers = _defaultSignatureHeadersProvider.ProvideDefaultHeaders(client.SignatureAlgorithm);
            }

            return Task.FromResult(newSignature);
        }
    }
}