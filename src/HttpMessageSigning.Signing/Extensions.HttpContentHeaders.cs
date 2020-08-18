using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        /// <summary>
        /// Adds or updates a content header with the specified name.
        /// </summary>
        /// <param name="headers">The header collection to add or update to.</param>
        /// <param name="name">The name of the header to add or update.</param>
        /// <param name="value">The single value to set.</param>
        public static void Set(this HttpContentHeaders headers, string name, string value) {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            headers.Set(name, new[] {value});
        }

        /// <summary>
        /// Adds or updates a content header with the specified name.
        /// </summary>
        /// <param name="headers">The header collection to add or update to.</param>
        /// <param name="name">The name of the header to add or update.</param>
        /// <param name="values">The values to set.</param>
        public static void Set(this HttpContentHeaders headers, string name, params string[] values) {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            headers.Set(name, (IEnumerable<string>) values);
        }

        /// <summary>
        /// Adds or updates a content header with the specified name.
        /// </summary>
        /// <param name="headers">The header collection to add or update to.</param>
        /// <param name="name">The name of the header to add or update.</param>
        /// <param name="values">The values to set.</param>
        public static void Set(this HttpContentHeaders headers, string name, IEnumerable<string> values) {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            if (headers.Contains(name)) headers.Remove(name);

            headers.Add(name, values);
        }
    }
}