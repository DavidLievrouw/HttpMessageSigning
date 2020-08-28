using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        /// <summary>
        ///     Perform URI decoding on the specified string.
        /// </summary>
        /// <param name="encoded">The string that might be encoded.</param>
        /// <returns>The decoded string.</returns>
        public static string UrlDecode(this string encoded) {
            return encoded == null
                ? null
                : Uri.UnescapeDataString(encoded);
        }

        /// <summary>
        ///     Perform URI encoding, according to RFC 3986, on the specified <see cref="string" /> representation of the <see cref="Uri" />.
        /// </summary>
        /// <param name="decoded">The <see cref="String" /> representation of the <see cref="Uri" /> that needs to be encoded.</param>
        /// <returns>The encoded string representation of the specified <see cref="String" /> representation of the <see cref="Uri" />, according to RFC 3986.</returns>
        public static string UrlEncode(this string decoded) {
            return decoded == null
                ? null
                : new Uri(decoded, UriKind.RelativeOrAbsolute).UrlEncode();
        }
    }
}