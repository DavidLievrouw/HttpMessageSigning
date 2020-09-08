using System;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    internal class UnescapedRequestTargetEscaper : IRequestTargetEscaper {
        public string Escape(Uri requestTarget, RequestTargetEscaping escaping) {
            if (escaping != RequestTargetEscaping.Unescaped) throw new ArgumentException($"This {GetType().Name} cannot escape adhering to the rules of option '{escaping}'.");
            
            if (requestTarget == null) throw new ArgumentNullException(nameof(requestTarget));

            return requestTarget.IsAbsoluteUri
                ? requestTarget.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped)
                : ("/" + requestTarget.OriginalString.Split('#')[0].TrimStart('/')).UriUnescape();
        }
    }
}