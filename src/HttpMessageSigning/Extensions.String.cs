using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        /// <summary>
        ///     Perform URI unescaping on the specified string.
        /// </summary>
        /// <param name="encoded">The string that might be encoded.</param>
        /// <returns>The unescaped string.</returns>
        public static string UriUnescape(this string encoded) {
            return encoded == null
                ? null
                : Uri.UnescapeDataString(encoded);
        }

        /// <summary>
        ///     Perform URI escaping, according to RFC 3986, on the specified <see cref="string" /> representation of the <see cref="Uri" />.
        /// </summary>
        /// <param name="unescaped">The <see cref="String" /> representation of the <see cref="Uri" /> that needs to be encoded.</param>
        /// <returns>The escaped string representation of the specified Uri <see cref="String" />, according to RFC 3986.</returns>
        public static string UriEscape(this string unescaped) {
            return unescaped == null
                ? null
                : new Uri(unescaped, UriKind.RelativeOrAbsolute).UriEscape();
        }
    }
}