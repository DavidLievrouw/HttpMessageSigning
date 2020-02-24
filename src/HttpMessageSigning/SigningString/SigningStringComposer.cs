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

        public string Compose(HttpRequestMessage request, SigningSettings settings) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            settings.Validate();
            
            if (!settings.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                settings.Headers = new[] {HeaderName.PredefinedHeaderNames.RequestTarget}.Concat(settings.Headers).ToArray();
            }

            if (!settings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                var requestTargetHeaderIdx = Array.IndexOf(settings.Headers, HeaderName.PredefinedHeaderNames.RequestTarget);
                settings.Headers = settings.Headers
                    .Take(requestTargetHeaderIdx + 1)
                    .Concat(new[] {HeaderName.PredefinedHeaderNames.Date})
                    .Concat(settings.Headers.Skip(requestTargetHeaderIdx + 1))
                    .ToArray();
            }
            
            var headerAppender = _headerAppenderFactory.Create(request, settings);

            var sb = new StringBuilder();
            foreach (var headerName in settings.Headers.Where(h => h != HeaderName.Empty)) {
                sb = sb.Append(headerAppender.BuildStringToAppend(headerName));
            }

            return sb.ToString();
        }
    }
}