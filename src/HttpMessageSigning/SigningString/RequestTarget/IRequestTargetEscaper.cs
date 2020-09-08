using System;

namespace Dalion.HttpMessageSigning.SigningString.RequestTarget {
    internal interface IRequestTargetEscaper {
        string Escape(Uri requestTarget, RequestTargetEscaping escaping);
    }
}