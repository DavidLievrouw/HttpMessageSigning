using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct HeaderName : IEquatable<HeaderName> {
        public static class PredefinedHeaderNames {
            public static readonly HeaderName RequestTarget = new HeaderName("(request-target)");
            public static readonly HeaderName Created = new HeaderName("(created)");
            public static readonly HeaderName Expires = new HeaderName("(expires)");
            public static readonly HeaderName Date = new HeaderName("date");
        }

        public HeaderName(string value) {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public static HeaderName Empty { get; } = new HeaderName(string.Empty);

        public bool Equals(HeaderName other) {
            return string.Equals(Value ?? string.Empty, other.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            return obj is HeaderName other && Equals(other);
        }

        public override int GetHashCode() {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);
        }

        public static bool operator ==(HeaderName left, HeaderName right) {
            return left.Equals(right);
        }

        public static bool operator !=(HeaderName left, HeaderName right) {
            return !left.Equals(right);
        }

        public static explicit operator HeaderName(string value) {
            return new HeaderName(value ?? string.Empty);
        }

        public static implicit operator string(HeaderName name) {
            return name.Value.ToLowerInvariant();
        }

        public override string ToString() {
            return (Value ?? string.Empty).ToLowerInvariant();
        }
    }
}