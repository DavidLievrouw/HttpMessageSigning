using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class AllHeadersPresentVerificationTask : IVerificationTask {
        public Task Verify(HttpRequestMessage signedRequest, Signature signature, Client client) {
            if (!signature.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                throw new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.RequestTarget} header is required to be included in the signature.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeDateHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                throw new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Date} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                throw new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Created} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                throw new SignatureVerificationException($"The {HeaderName.PredefinedHeaderNames.Expires} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            foreach (var headerName in signature.Headers) {
                if (!signedRequest.Headers.Contains(headerName)) {
                    throw new SignatureVerificationException($"The request header {headerName} is missing, but it is required to validate the signature.");
                }
            }
            
            return Task.CompletedTask;
        }
    }
}