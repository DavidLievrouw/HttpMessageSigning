using System;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class SignedRequestAuthenticationHandler : AuthenticationHandler<SignedRequestAuthenticationOptions> {
        private readonly IRequestSignatureVerifier _requestSignatureVerifier;

        public SignedRequestAuthenticationHandler(
            IOptionsMonitor<SignedRequestAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            Microsoft.AspNetCore.Authentication.ISystemClock clock,
            IRequestSignatureVerifier requestSignatureVerifier) : base(options, logger, encoder, clock) {
            _requestSignatureVerifier = requestSignatureVerifier ?? throw new ArgumentNullException(nameof(requestSignatureVerifier));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!Request.Headers.ContainsKey("Authorization")) return AuthenticateResult.NoResult();
            if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var headerValue)) return AuthenticateResult.NoResult();
            if (!Scheme.Name.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();

            var verificationResult = await _requestSignatureVerifier.VerifySignature(Request);

            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var ticket = new AuthenticationTicket(successResult.Principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                return AuthenticateResult.Fail(failureResult.SignatureVerificationException);
            }

            return AuthenticateResult.Fail("Request signature verification failed.");
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties) {
            Response.Headers["WWW-Authenticate"] = $"{Scheme.Name} realm=\"{Options.Realm}\"";
            await base.HandleChallengeAsync(properties);
        }
    }
}