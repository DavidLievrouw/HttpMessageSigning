using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct StringKeyId : IEquatable<StringKeyId>, IKeyId {
        public StringKeyId(string value) {
            Key = value ?? string.Empty;
        }

        public string Key { get; }

        public static StringKeyId Empty { get; } = new StringKeyId(string.Empty);

        public bool Equals(StringKeyId other) {
            return string.Equals(Key ?? string.Empty, other.Key ?? string.Empty);
        }

        public override bool Equals(object obj) {
            return obj is StringKeyId other && Equals(other);
        }

        public override int GetHashCode() {
            return (Key ?? string.Empty).GetHashCode();
        }

        public static bool operator ==(StringKeyId left, StringKeyId right) {
            return left.Equals(right);
        }

        public static bool operator !=(StringKeyId left, StringKeyId right) {
            return !left.Equals(right);
        }

        public static explicit operator StringKeyId(string value) {
            return new StringKeyId(value ?? string.Empty);
        }

        public static implicit operator string(StringKeyId id) {
            return id.Key;
        }

        public override string ToString() {
            return Key ?? string.Empty;
        }
    }
}