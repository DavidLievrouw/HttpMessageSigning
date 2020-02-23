using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct KeyId : IEquatable<KeyId> {
        public KeyId(string value) {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public static KeyId Empty { get; } = new KeyId(string.Empty);

        public bool Equals(KeyId other) {
            return string.Equals(Value ?? string.Empty, other.Value ?? string.Empty);
        }

        public override bool Equals(object obj) {
            return obj is KeyId other && Equals(other);
        }

        public override int GetHashCode() {
            return (Value ?? string.Empty).GetHashCode();
        }

        public static bool operator ==(KeyId left, KeyId right) {
            return left.Equals(right);
        }

        public static bool operator !=(KeyId left, KeyId right) {
            return !left.Equals(right);
        }

        public static explicit operator KeyId(string value) {
            return new KeyId(value ?? string.Empty);
        }

        public static implicit operator string(KeyId id) {
            return id.Value;
        }

        public override string ToString() {
            return Value ?? string.Empty;
        }
    }
}