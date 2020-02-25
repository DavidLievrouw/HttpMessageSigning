using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an HMAC secret that is used to sign a request, or to validate a signature.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class HMACSecret : Secret, IEquatable<HMACSecret> {
        public HMACSecret(string value) {
            Value = value ?? string.Empty;
        }
        
        public string Value { get; }

        public static HMACSecret Empty { get; } = new HMACSecret(string.Empty);

        public bool Equals(HMACSecret other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is HMACSecret other && Equals(other);
        }

        public override int GetHashCode() {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(HMACSecret left, HMACSecret right) {
            return Equals(left, right);
        }

        public static bool operator !=(HMACSecret left, HMACSecret right) {
            return !Equals(left, right);
        }

        public static bool TryParse(string value, out HMACSecret parsed) {
            parsed = new HMACSecret(value);
            return true;
        }
        
        public static explicit operator HMACSecret(string value) {
            if (!TryParse(value, out var secret)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of an HMAC secret.");
            }
            return secret;
        }
        
        public static HMACSecret Parse(string value) {
            return (HMACSecret) value;
        }
        
        public static implicit operator string(HMACSecret id) {
            return id.ToString();
        }

        public override string ToString() {
            return Value ?? string.Empty;
        }

        public override SignatureAlgorithm Algorithm => SignatureAlgorithm.HMAC;
    }
}