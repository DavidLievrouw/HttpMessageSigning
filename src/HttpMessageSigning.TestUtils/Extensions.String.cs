using System;

namespace Dalion.HttpMessageSigning.TestUtils {
    public static partial class Extensions {
        public static Uri ToUri(this string str) {
            if (str == null) return null;
            if (str == string.Empty) return new Uri("/", UriKind.Relative);

            return str.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? new Uri(str, UriKind.Absolute)
                : new Uri("/" + str.TrimStart('/'), UriKind.Relative);
        }
    }
}