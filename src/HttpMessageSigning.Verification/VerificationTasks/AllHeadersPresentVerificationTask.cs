using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class AllHeadersPresentVerificationTask : IVerificationTask {
        public Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                return SignatureVerificationFailure.HeaderMissing($"The {HeaderName.PredefinedHeaderNames.RequestTarget} header is required to be included in the signature.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeDateHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Date} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Created} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Expires} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            foreach (var headerName in signature.Headers) {
                if (headerName != HeaderName.PredefinedHeaderNames.RequestTarget) {
                    if (!signedRequest.Headers.Contains(headerName)) {
                        return SignatureVerificationFailure.HeaderMissing($"The request header {headerName} is missing, but it is required to validate the signature.")
                            .ToTask<SignatureVerificationFailure>();
                    }
                }
            }
            
            return Task.FromResult<SignatureVerificationFailure>(null);
        }
    }
}