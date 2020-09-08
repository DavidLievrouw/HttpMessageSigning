using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    internal class OriginalStringRequestTargetEscaper : IRequestTargetEscaper {
        public string Escape(Uri requestTarget, RequestTargetEscaping escaping) {
            if (escaping != RequestTargetEscaping.OriginalString) throw new ArgumentException($"This {GetType().Name} cannot escape adhering to the rules of option '{escaping}'.");

            if (requestTarget == null) throw new ArgumentNullException(nameof(requestTarget));

            if (requestTarget.IsAbsoluteUri) {
                var segments = requestTarget.OriginalString.Split('#')[0].Split('/');
                return "/" + string.Join("/", segments.Skip(3));
            }

            return "/" + requestTarget.OriginalString.Split('#')[0].TrimStart('/');
        }
    }
}