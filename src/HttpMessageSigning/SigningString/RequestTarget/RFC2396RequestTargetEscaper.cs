using System;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    internal class RFC2396RequestTargetEscaper : IRequestTargetEscaper {
        public string Escape(Uri requestTarget, RequestTargetEscaping escaping) {
            if (escaping != RequestTargetEscaping.RFC2396) throw new ArgumentException($"This {GetType().Name} cannot escape adhering to the rules of option '{escaping}'.");
            
            if (requestTarget == null) throw new ArgumentNullException(nameof(requestTarget));

            var unEscapedPathAndQuery = requestTarget.IsAbsoluteUri
                ? requestTarget.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)
                : requestTarget.OriginalString.Split('#')[0].UriUnescape();

            return unEscapedPathAndQuery.UriEscape(UriEscaping.RFC2396);
        }
    }
}