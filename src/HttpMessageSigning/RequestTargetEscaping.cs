using System;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Defines the methods of escaping of the (request-target) pseudo-header value.
    /// </summary>
    public enum RequestTargetEscaping {
        /// <summary>
        ///     Follow the rules of RFC 3986.
        /// </summary>
        RFC3986,

        /// <summary>
        ///     Follow the rules of RFC 2396, obsoleted by RFC 3986.
        /// </summary>
        RFC2396,

        /// <summary>
        ///     Take the original <see cref="Uri" /> string, and leave it up to the creator.
        /// </summary>
        OriginalString,

        /// <summary>
        ///     Any escaping is removed.
        /// </summary>
        Unescaped
    }
}