using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents a secret that is used to sign a request, or to validate a signature.
    /// </summary>
    /// <remarks>In the case of symmetric keys, this value represents the shared key. In case of an asymmetric key, this value represents the private key of the client.</remarks>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct Secret : IEquatable<Secret> {
        public Secret(string value) {
            Value = value ?? string.Empty;
        }
        
        public string Value { get; }

        public static Secret Empty { get; } = new Secret(string.Empty);
        
        public bool Equals(Secret other) {
            return (Value ?? string.Empty) == other.Value;
        }

        public override bool Equals(object obj) {
            return obj is Secret other && Equals(other);
        }

        public override int GetHashCode() {
            return (Value ?? string.Empty).GetHashCode();
        }

        public static bool operator ==(Secret left, Secret right) {
            return left.Equals(right);
        }

        public static bool operator !=(Secret left, Secret right) {
            return !left.Equals(right);
        }

        public static bool TryParse(string value, out Secret parsed) {
            parsed = new Secret(value);
            return true;
        }
        
        public static explicit operator Secret(string value) {
            if (!TryParse(value, out var secret)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a secret.");
            }
            return secret;
        }
        
        public static Secret Parse(string value) {
            return (Secret) value;
        }
        
        public static implicit operator string(Secret id) {
            return id.ToString();
        }

        public override string ToString() {
            return Value ?? string.Empty;
        }
    }
}