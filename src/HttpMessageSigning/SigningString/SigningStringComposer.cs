using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class SigningStringComposer : ISigningStringComposer {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;

        public SigningStringComposer(IHeaderAppenderFactory headerAppenderFactory) {
            _headerAppenderFactory = headerAppenderFactory ?? throw new ArgumentNullException(nameof(headerAppenderFactory));
        }

        public string Compose(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfComposing) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            settings.Validate();
            
            // According to the spec, the header (request-target) should always be a part of the signature string.
            if (!settings.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                settings.Headers = new[] {HeaderName.PredefinedHeaderNames.RequestTarget}.Concat(settings.Headers).ToArray();
            }

            // According to the spec, when the algorithm starts with 'rsa', 'hmac' or 'ecdsa', the Date header should be part of the signature string.
            if (ShouldHaveDateHeader(settings.SignatureAlgorithm) && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                var requestTargetHeaderIdx = Array.IndexOf(settings.Headers, HeaderName.PredefinedHeaderNames.RequestTarget);
                settings.Headers = settings.Headers
                    .Take(requestTargetHeaderIdx + 1)
                    .Concat(new[] {HeaderName.PredefinedHeaderNames.Date})
                    .Concat(settings.Headers.Skip(requestTargetHeaderIdx + 1))
                    .ToArray();
            }            
            
            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (created) header should be part of the signature string.
            if (!ShouldHaveDateHeader(settings.SignatureAlgorithm) && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                var requestTargetHeaderIdx = Array.IndexOf(settings.Headers, HeaderName.PredefinedHeaderNames.RequestTarget);
                settings.Headers = settings.Headers
                    .Take(requestTargetHeaderIdx + 1)
                    .Concat(new[] {HeaderName.PredefinedHeaderNames.Created})
                    .Concat(settings.Headers.Skip(requestTargetHeaderIdx + 1))
                    .ToArray();
            }

            // When digest is enabled, make it part of the signature string
            if (settings.DigestHashAlgorithm != HashAlgorithm.None && request.Method.HasBody() && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                settings.Headers = settings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Digest}).ToArray();
            }
            
            var headerAppender = _headerAppenderFactory.Create(request, settings, timeOfComposing);

            var sb = new StringBuilder();
            foreach (var headerName in settings.Headers.Where(h => h != HeaderName.Empty)) {
                sb = sb.Append(headerAppender.BuildStringToAppend(headerName));
            }

            return sb.ToString().Trim();
        }

        private static bool ShouldHaveDateHeader(SignatureAlgorithm signatureAlgorithm) {
            var signatureAlgorithmString = signatureAlgorithm.ToString().ToLowerInvariant();
            return signatureAlgorithmString.StartsWith("rsa") || 
                   signatureAlgorithmString.StartsWith("hmac") ||
                   signatureAlgorithmString.StartsWith("ecdsa");
        }
    }
}