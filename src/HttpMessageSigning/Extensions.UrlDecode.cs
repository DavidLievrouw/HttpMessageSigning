using System;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        public static string UrlDecode(this string encoded) {
            return encoded == null
                ? null
                : Uri.UnescapeDataString(encoded);
        }
    }
}