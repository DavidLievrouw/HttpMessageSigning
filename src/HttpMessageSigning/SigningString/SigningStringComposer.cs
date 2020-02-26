using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class SigningStringComposer : ISigningStringComposer {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;

        public SigningStringComposer(IHeaderAppenderFactory headerAppenderFactory) {
            _headerAppenderFactory = headerAppenderFactory ?? throw new ArgumentNullException(nameof(headerAppenderFactory));
        }

        public string Compose(HttpRequestForSigning request, SigningSettings settings, DateTimeOffset timeOfComposing) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            settings.Validate();
            
            // According to the spec, the header (request-target) should always be a part of the signature string.
            if (!settings.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                settings.Headers = new[] {HeaderName.PredefinedHeaderNames.RequestTarget}.Concat(settings.Headers).ToArray();
            }

            // According to the spec, when the algorithm starts with 'rsa', 'hmac' or 'ecdsa', the Date header should be part of the signature string.
            if (settings.SignatureAlgorithm.ShouldIncludeDateHeader() && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                settings.Headers = settings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Date}).ToArray();
            }            
            
            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (created) header should be part of the signature string.
            if (settings.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                settings.Headers = settings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (expires) header should be part of the signature string.
            if (settings.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                settings.Headers = settings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();
            }
            
            // When digest is enabled, make it part of the signature string
            if (!string.IsNullOrEmpty(settings.DigestHashAlgorithm.Name) && request.Method.SupportsBody() && !settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                settings.Headers = settings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Digest}).ToArray();
            }
            
            var headerAppender = _headerAppenderFactory.Create(request, timeOfComposing);

            var sb = new StringBuilder();
            foreach (var headerName in settings.Headers.Where(h => h != HeaderName.Empty)) {
                sb = sb.Append(headerAppender.BuildStringToAppend(headerName));
            }

            return sb.ToString().Trim();
        }
    }
}