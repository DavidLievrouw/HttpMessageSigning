using System;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Defines the different supported rule sets for escaping <see cref="Uri" /> instances.
    /// </summary>
    public enum UriEscaping {
        /// <summary>
        ///     Follow the rules of RFC 3986.
        /// </summary>
        RFC3986,

        /// <summary>
        ///     Follow the rules of RFC 2396, obsoleted by RFC 3986.
        /// </summary>
        RFC2396
    }
}