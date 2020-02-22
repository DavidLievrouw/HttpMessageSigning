using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct KeyId : IEquatable<KeyId> {
        private string _value;

        public KeyId(string value) {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value => _value ?? (_value = string.Empty);

        public static KeyId Empty { get; } = new KeyId(string.Empty);

        public bool Equals(KeyId other) {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj) {
            return obj is KeyId other && Equals(other);
        }

        public override int GetHashCode() {
            return Value != null ? Value.GetHashCode() : 0;
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
            return Value;
        }
    }
}