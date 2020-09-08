using System;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    internal class CompositeRequestTargetEscaper : IRequestTargetEscaper {
        private readonly IRequestTargetEscaper _originalStringEscaper;
        private readonly IRequestTargetEscaper _rfc2396Escaper;
        private readonly IRequestTargetEscaper _rfc3986Escaper;
        private readonly IRequestTargetEscaper _unescapedEscaper;

        public CompositeRequestTargetEscaper(
            IRequestTargetEscaper rfc3986Escaper,
            IRequestTargetEscaper rfc2396Escaper,
            IRequestTargetEscaper unescapedEscaper,
            IRequestTargetEscaper originalStringEscaper) {
            _rfc3986Escaper = rfc3986Escaper ?? throw new ArgumentNullException(nameof(rfc3986Escaper));
            _rfc2396Escaper = rfc2396Escaper ?? throw new ArgumentNullException(nameof(rfc2396Escaper));
            _unescapedEscaper = unescapedEscaper ?? throw new ArgumentNullException(nameof(unescapedEscaper));
            _originalStringEscaper = originalStringEscaper ?? throw new ArgumentNullException(nameof(originalStringEscaper));
        }

        public string Escape(Uri requestTarget, RequestTargetEscaping escaping) {
            if (requestTarget == null) throw new ArgumentNullException(nameof(requestTarget));

            switch (escaping) {
                case RequestTargetEscaping.RFC3986:
                    return _rfc3986Escaper.Escape(requestTarget, escaping);
                case RequestTargetEscaping.RFC2396:
                    return _rfc2396Escaper.Escape(requestTarget, escaping);
                case RequestTargetEscaping.OriginalString:
                    return _originalStringEscaper.Escape(requestTarget, escaping);
                case RequestTargetEscaping.Unescaped:
                    return _unescapedEscaper.Escape(requestTarget, escaping);
                default:
                    throw new ArgumentOutOfRangeException(nameof(escaping), escaping, $"The specified {nameof(RequestTargetEscaping)} value is currently not supported.");
            }
        }
    }
}