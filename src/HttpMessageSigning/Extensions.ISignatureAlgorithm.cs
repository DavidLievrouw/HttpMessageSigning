using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static bool ShouldIncludeDateHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLower();
            return signatureAlgorithmString.StartsWith("rsa", StringComparison.Ordinal) || 
                   signatureAlgorithmString.StartsWith("hmac", StringComparison.Ordinal) ||
                   signatureAlgorithmString.StartsWith("ecdsa", StringComparison.Ordinal);
        }        
        
        internal static bool ShouldIncludeCreatedHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLower();
            return !signatureAlgorithmString.StartsWith("rsa", StringComparison.Ordinal) && 
                   !signatureAlgorithmString.StartsWith("hmac", StringComparison.Ordinal) &&
                   !signatureAlgorithmString.StartsWith("ecdsa", StringComparison.Ordinal);
        }
        
        internal static bool ShouldIncludeExpiresHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLower();
            return !signatureAlgorithmString.StartsWith("rsa", StringComparison.Ordinal) && 
                   !signatureAlgorithmString.StartsWith("hmac", StringComparison.Ordinal) &&
                   !signatureAlgorithmString.StartsWith("ecdsa", StringComparison.Ordinal);
        }
    }
}