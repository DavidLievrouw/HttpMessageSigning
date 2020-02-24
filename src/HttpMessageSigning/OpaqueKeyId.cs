using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct OpaqueKeyId : IKeyId, IEquatable<OpaqueKeyId> {
        public OpaqueKeyId(string value) {
            Value = value ?? string.Empty;
        }
        
        public string Value { get; }

        public static OpaqueKeyId Empty { get; } = new OpaqueKeyId(string.Empty);
        
        public bool Equals(OpaqueKeyId other) {
            return (Value ?? string.Empty) == other.Value;
        }

        public override bool Equals(object obj) {
            return obj is OpaqueKeyId other && Equals(other);
        }

        public override int GetHashCode() {
            return (Value ?? string.Empty).GetHashCode();
        }

        public static bool operator ==(OpaqueKeyId left, OpaqueKeyId right) {
            return left.Equals(right);
        }

        public static bool operator !=(OpaqueKeyId left, OpaqueKeyId right) {
            return !left.Equals(right);
        }

        public static bool TryParse(string value, out OpaqueKeyId parsed) {
            parsed = new OpaqueKeyId(value);
            return true;
        }
        
        public static explicit operator OpaqueKeyId(string value) {
            if (!TryParse(value, out var keyId)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a key id.");
            }
            return keyId;
        }
        
        public static OpaqueKeyId Parse(string value) {
            return (OpaqueKeyId) value;
        }
        
        public static implicit operator string(OpaqueKeyId id) {
            return id.ToString();
        }

        public override string ToString() {
            return Value ?? string.Empty;
        }
    }
}