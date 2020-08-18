using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    /// <summary>
    /// The HttpMessageSigning Owin authentication middleware.
    /// </summary>
    public class HttpRequestSignatureAuthenticationMiddleware : AuthenticationMiddleware<SignedHttpRequestAuthenticationOptions> {
        /// <inheritdoc />
        public HttpRequestSignatureAuthenticationMiddleware(
            OwinMiddleware next,
            SignedHttpRequestAuthenticationOptions options) : base(next, options) {
            options.Validate();
        }

        /// <inheritdoc />
        protected override AuthenticationHandler<SignedHttpRequestAuthenticationOptions> CreateHandler() {
            return new SignedHttpRequestAuthenticationHandler();
        }
    }
}