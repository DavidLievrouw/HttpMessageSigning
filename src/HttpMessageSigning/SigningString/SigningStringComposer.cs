using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class SigningStringComposer : ISigningStringComposer {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;
        private readonly INonceAppender _nonceAppender;

        public SigningStringComposer(IHeaderAppenderFactory headerAppenderFactory, INonceAppender nonceAppender) {
            _headerAppenderFactory = headerAppenderFactory ?? throw new ArgumentNullException(nameof(headerAppenderFactory));
            _nonceAppender = nonceAppender ?? throw new ArgumentNullException(nameof(nonceAppender));
        }

        public string Compose(SigningStringCompositionRequest compositionRequest) {
            if (compositionRequest == null) throw new ArgumentNullException(nameof(compositionRequest));
            
            var headerAppender = _headerAppenderFactory.Create(
                compositionRequest.Request, 
                compositionRequest.RequestTargetEscaping, 
                compositionRequest.TimeOfComposing,
                compositionRequest.Expires);

            var sb = new StringBuilder();
            foreach (var headerName in compositionRequest.HeadersToInclude.Where(h => h != HeaderName.Empty)) {
                headerAppender.Append(headerName, sb);
            }

            _nonceAppender.Append(compositionRequest.Nonce, sb);
            
            return sb.ToString().TrimStart();
        }
    }
}