using System.Linq;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class AllHeadersPresentVerificationTask : VerificationTask {
        private static readonly HeaderName[] PseudoHeaders = {
            HeaderName.PredefinedHeaderNames.RequestTarget,
            HeaderName.PredefinedHeaderNames.Created,
            HeaderName.PredefinedHeaderNames.Expires
        };
        
        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                return SignatureVerificationFailure.HeaderMissing($"The {HeaderName.PredefinedHeaderNames.RequestTarget} header is required to be included in the signature.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeDateHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Date} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Created} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Expires} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }

            foreach (var headerName in signature.Headers) {
                var isPresent = PseudoHeaders.Contains(headerName);

                if (!isPresent) {
                    isPresent = signedRequest.Headers.Contains(headerName);
                }
                
                if (!isPresent) {
                    return SignatureVerificationFailure.HeaderMissing($"The request header {headerName} is missing, but it is required to validate the signature.");
                }
            }
            
            return null;
        }
    }
}