using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Dalion.HttpMessageSigning.Signing {
    public static partial class Extensions {
        public static void Set(this HttpRequestHeaders headers, string name, string value) {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            headers.Set(name, new[] {value});
        }

        public static void Set(this HttpRequestHeaders headers, string name, params string[] values) {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            headers.Set(name, (IEnumerable<string>) values);
        }

        public static void Set(this HttpRequestHeaders headers, string name, IEnumerable<string> values) {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            if (headers.Contains(name)) headers.Remove(name);

            headers.Add(name, values);
        }
    }
}