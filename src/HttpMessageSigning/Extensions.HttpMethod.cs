using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        internal static bool SupportsBody(this HttpMethod method) {
            if (method == null) throw new ArgumentNullException(nameof(method));

            return method != HttpMethod.Get &&
                   method != HttpMethod.Delete &&
                   method != HttpMethod.Trace &&
                   method != HttpMethod.Head;
        }
    }
}