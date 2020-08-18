using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        /// <summary>
        ///     Perform URL decoding on the specified string.
        /// </summary>
        /// <param name="encoded">The string that might be encoded.</param>
        /// <returns>The decoded string.</returns>
        public static string UrlDecode(this string encoded) {
            return encoded == null
                ? null
                : Uri.UnescapeDataString(encoded);
        }
    }
}