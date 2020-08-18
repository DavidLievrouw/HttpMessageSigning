using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents a pointer to a header of the http request message.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public readonly struct HeaderName : IEquatable<HeaderName> {
        /// <summary>
        ///     Creates a new <see cref="HeaderName" />.
        /// </summary>
        /// <param name="value">The string representation of this instance.</param>
        public HeaderName(string value) {
            Value = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets the string representation of this instance.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Represents an empty <see cref="HeaderName" />.
        /// </summary>
        public static HeaderName Empty { get; } = new HeaderName(string.Empty);

        /// <inheritdoc />
        public bool Equals(HeaderName other) {
            return string.Equals(Value ?? string.Empty, other.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            return obj is HeaderName other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);
        }

        /// <summary>
        ///     The equality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are equal, otherwise false.</returns>
        public static bool operator ==(HeaderName left, HeaderName right) {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are not equal, otherwise false.</returns>
        public static bool operator !=(HeaderName left, HeaderName right) {
            return !left.Equals(right);
        }

        /// <summary>
        ///     An explicit conversion operator for this type from string.
        /// </summary>
        public static explicit operator HeaderName(string value) {
            return new HeaderName(value ?? string.Empty);
        }

        /// <summary>
        ///     An implicit conversion operator for this type to string.
        /// </summary>
        public static implicit operator string(HeaderName name) {
            return name.Value.ToLowerInvariant();
        }

        /// <inheritdoc />
        public override string ToString() {
            return (Value ?? string.Empty).ToLowerInvariant();
        }

        /// <summary>
        ///     Defines the predefined header names.
        /// </summary>
        public static class PredefinedHeaderNames {
            /// <summary>
            ///     The (request-target) pseudo-header.
            /// </summary>
            public static readonly HeaderName RequestTarget = new HeaderName("(request-target)");

            /// <summary>
            ///     The (created) pseudo-header.
            /// </summary>
            public static readonly HeaderName Created = new HeaderName("(created)");

            /// <summary>
            ///     The (expires) pseudo-header.
            /// </summary>
            public static readonly HeaderName Expires = new HeaderName("(expires)");

            /// <summary>
            ///     The Date header.
            /// </summary>
            public static readonly HeaderName Date = new HeaderName("date");

            /// <summary>
            ///     The Digest header.
            /// </summary>
            public static readonly HeaderName Digest = new HeaderName("digest");
        }
    }
}