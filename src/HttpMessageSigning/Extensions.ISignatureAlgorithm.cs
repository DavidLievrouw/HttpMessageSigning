using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static bool ShouldIncludeDateHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLowerInvariant();
            return signatureAlgorithmString.StartsWith("rsa") || 
                   signatureAlgorithmString.StartsWith("hmac") ||
                   signatureAlgorithmString.StartsWith("ecdsa");
        }        
        
        internal static bool ShouldIncludeCreatedHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLowerInvariant();
            return !signatureAlgorithmString.StartsWith("rsa") && 
                   !signatureAlgorithmString.StartsWith("hmac") &&
                   !signatureAlgorithmString.StartsWith("ecdsa");
        }
        
        internal static bool ShouldIncludeExpiresHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLowerInvariant();
            return !signatureAlgorithmString.StartsWith("rsa") && 
                   !signatureAlgorithmString.StartsWith("hmac") &&
                   !signatureAlgorithmString.StartsWith("ecdsa");
        }
    }
}