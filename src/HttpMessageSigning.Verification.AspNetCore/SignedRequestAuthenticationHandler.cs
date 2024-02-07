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
        private readonly IAuthenticationHeaderExtractor _authenticationHeaderExtractor;
        
#if NET8_0_OR_GREATER
        public SignedRequestAuthenticationHandler(
            IOptionsMonitor<SignedRequestAuthenticationOptions> options,
            UrlEncoder encoder,
            IRequestSignatureVerifier requestSignatureVerifier,
            IAuthenticationHeaderExtractor authenticationHeaderExtractor,
            ILoggerFactory loggerFactory = null) : base(options, loggerFactory, encoder) {
#else
        public SignedRequestAuthenticationHandler(
            IOptionsMonitor<SignedRequestAuthenticationOptions> options,
            UrlEncoder encoder,
            ISystemClock clock,
            IRequestSignatureVerifier requestSignatureVerifier,
            IAuthenticationHeaderExtractor authenticationHeaderExtractor,
            ILoggerFactory loggerFactory = null) : base(options, loggerFactory, encoder, clock) {
#endif
            _requestSignatureVerifier = requestSignatureVerifier ?? throw new ArgumentNullException(nameof(requestSignatureVerifier));
            _authenticationHeaderExtractor = authenticationHeaderExtractor ?? throw new ArgumentNullException(nameof(authenticationHeaderExtractor));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            var authHeader = _authenticationHeaderExtractor.Extract(Request);
            if (authHeader == null) return AuthenticateResult.NoResult();
            if (!Scheme.Name.Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();
            if (string.IsNullOrEmpty(authHeader.Scheme) || string.IsNullOrEmpty(authHeader.Parameter)) return AuthenticateResult.NoResult();

            var verificationResult = await _requestSignatureVerifier.VerifySignature(Request, Options).ConfigureAwait(continueOnCapturedContext: false);

            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var onIdentityVerifiedTask = Options.OnIdentityVerified?.Invoke(Request, successResult);
                if (onIdentityVerifiedTask != null) await onIdentityVerifiedTask.ConfigureAwait(continueOnCapturedContext: false);
                
                var ticket = new AuthenticationTicket(successResult.Principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                var onIdentityVerificationFailedTask = Options.OnIdentityVerificationFailed?.Invoke(Request, failureResult);
                if (onIdentityVerificationFailedTask != null) await onIdentityVerificationFailedTask.ConfigureAwait(continueOnCapturedContext: false);
                
                return AuthenticateResult.Fail(new SignatureVerificationException(failureResult.Failure.ToString()));
            }

            return AuthenticateResult.Fail("Request signature verification failed.");
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties) {
            Response.Headers["WWW-Authenticate"] = $"{Scheme.Name} realm=\"{Options.Realm}\"";
            if (Response.StatusCode == 200) {
                return base.HandleChallengeAsync(properties);
            }
            return Task.CompletedTask;
        }
    }
}