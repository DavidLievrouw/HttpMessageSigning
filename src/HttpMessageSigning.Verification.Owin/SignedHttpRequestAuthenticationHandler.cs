using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class SignedHttpRequestAuthenticationHandler : AuthenticationHandler<SignedHttpRequestAuthenticationOptions> {
        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync() {
            if (!Request.Headers.ContainsKey("Authorization")) return null;
            if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var headerValue)) return null;
            if (!Options.Scheme.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase)) return null;

            var httpRequest = Request.ToHttpRequest();
            try {
                var verificationResult = await Options.RequestSignatureVerifier.VerifySignature(Request.ToHttpRequest());

                if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                    var onIdentityVerifiedTask = Options.OnIdentityVerified?.Invoke(successResult);
                    if (onIdentityVerifiedTask != null) await onIdentityVerifiedTask;

                    var ticket = new AuthenticationTicket(
                        (ClaimsIdentity) successResult.Principal.Identity,
                        new AuthenticationProperties());

                    return ticket;
                }

                if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                    var onIdentityVerificationFailedTask = Options.OnIdentityVerificationFailed?.Invoke(failureResult);
                    if (onIdentityVerificationFailedTask != null) await onIdentityVerificationFailedTask;

                    return null;
                }

                return null;
            }
            finally {
                httpRequest?.Body?.Dispose();
            }
        }

        protected override Task ApplyResponseChallengeAsync() {
            if (Response.StatusCode == 401) {
                var signatureScheme = $"{Options.Scheme} realm=\"{Options.Realm}\"";
                var value = Response.Headers["WWW-Authenticate"];
                if (string.IsNullOrEmpty(value)) {
                    value = signatureScheme;
                }
                else {
                    value += $", {signatureScheme}";
                }

                Response.Headers["WWW-Authenticate"] = value;
            }

            return base.ApplyResponseChallengeAsync();
        }
    }
}