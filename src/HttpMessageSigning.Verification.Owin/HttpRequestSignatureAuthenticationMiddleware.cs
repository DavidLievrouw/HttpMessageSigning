using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class HttpRequestSignatureAuthenticationMiddleware : AuthenticationMiddleware<SignedHttpRequestAuthenticationOptions> {
        public HttpRequestSignatureAuthenticationMiddleware(
            OwinMiddleware next,
            SignedHttpRequestAuthenticationOptions options) : base(next, options) {
            options.Validate();
        }

        protected override AuthenticationHandler<SignedHttpRequestAuthenticationOptions> CreateHandler() {
            return new SignedHttpRequestAuthenticationHandler();
        }
    }
}