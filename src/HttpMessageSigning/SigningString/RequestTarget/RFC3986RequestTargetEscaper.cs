using System;
using System.Diagnostics.CodeAnalysis;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    internal class RFC3986RequestTargetEscaper : IRequestTargetEscaper {
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public string Escape(Uri requestTarget, RequestTargetEscaping escaping) {
            if (escaping != RequestTargetEscaping.RFC3986) throw new ArgumentException($"This {GetType().Name} cannot escape adhering to the rules of option '{escaping}'.");
            
            if (requestTarget == null) throw new ArgumentNullException(nameof(requestTarget));

            var unEscapedPathAndQuery = requestTarget.IsAbsoluteUri
                ? requestTarget.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)
                : requestTarget.OriginalString.Split('#')[0].UriUnescape();

            return unEscapedPathAndQuery.UriEscape(UriEscaping.RFC3986);
        }
    }
}