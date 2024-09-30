using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static bool ShouldIncludeDateHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            return signatureAlgorithm.Name.StartsWith("rsa", StringComparison.OrdinalIgnoreCase) || 
                   signatureAlgorithm.Name.StartsWith("hmac", StringComparison.OrdinalIgnoreCase) ||
                   signatureAlgorithm.Name.StartsWith("ecdsa", StringComparison.OrdinalIgnoreCase);
        }        
        
        internal static bool ShouldIncludeCreatedHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            return !signatureAlgorithm.Name.StartsWith("rsa", StringComparison.OrdinalIgnoreCase) && 
                   !signatureAlgorithm.Name.StartsWith("hmac", StringComparison.OrdinalIgnoreCase) &&
                   !signatureAlgorithm.Name.StartsWith("ecdsa", StringComparison.OrdinalIgnoreCase);
        }
        
        internal static bool ShouldIncludeExpiresHeader(this ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));
            
            return !signatureAlgorithm.Name.StartsWith("rsa", StringComparison.OrdinalIgnoreCase) && 
                   !signatureAlgorithm.Name.StartsWith("hmac", StringComparison.OrdinalIgnoreCase) &&
                   !signatureAlgorithm.Name.StartsWith("ecdsa", StringComparison.OrdinalIgnoreCase);
        }
    }
}