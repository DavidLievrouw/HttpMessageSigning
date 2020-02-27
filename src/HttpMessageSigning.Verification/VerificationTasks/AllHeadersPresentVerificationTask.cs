using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class AllHeadersPresentVerificationTask : IVerificationTask {
        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                return new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.RequestTarget} header is required to be included in the signature.")
                    .ToTask<Exception>();
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeDateHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                return new SignatureVerificationException($"The inclusion of the {HeaderName.PredefinedHeaderNames.Date} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.")
                    .ToTask<Exception>();
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                return new SignatureVerificationException($"The inclusion of the {HeaderName.PredefinedHeaderNames.Created} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.")
                    .ToTask<Exception>();
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                return new SignatureVerificationException($"The inclusion of the {HeaderName.PredefinedHeaderNames.Expires} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.")
                    .ToTask<Exception>();
            }
            
            foreach (var headerName in signature.Headers) {
                if (headerName != HeaderName.PredefinedHeaderNames.RequestTarget) {
                    if (!signedRequest.Headers.Contains(headerName)) {
                        return new SignatureVerificationException($"The request header {headerName} is missing, but it is required to validate the signature.")
                            .ToTask<Exception>();
                    }
                }
            }
            
            return Task.FromResult<Exception>(null);
        }
    }
}