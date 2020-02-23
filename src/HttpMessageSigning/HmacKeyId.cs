using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct HmacKeyId : IEquatable<HmacKeyId>, IKeyId {
        private const string Prefix = "hmac_";

        public HmacKeyId(string key) {
            Key = key ?? string.Empty;
        }
        
        public string Key { get; }

        public static HmacKeyId Empty { get; } = new HmacKeyId(string.Empty);

        public bool Equals(HmacKeyId other) {
            return string.Equals(Key ?? string.Empty, other.Key ?? string.Empty);
        }

        public override bool Equals(object obj) {
            return obj is HmacKeyId other && Equals(other);
        }

        public override int GetHashCode() {
            return (Key ?? string.Empty).GetHashCode();
        }

        public static bool operator ==(HmacKeyId left, HmacKeyId right) {
            return left.Equals(right);
        }

        public static bool operator !=(HmacKeyId left, HmacKeyId right) {
            return !left.Equals(right);
        }

        public static explicit operator HmacKeyId(string value) {
            if (string.IsNullOrEmpty(value)) throw new FormatException("The specified hmac key id value is null or empty.");
            if (!value.StartsWith(Prefix)) throw new FormatException($"The specified value ({value}) is not a valid string representation of a hmac key id.");
            // ToDo: What if only hmac_?
            var key = value.Substring(Prefix.Length);
            return new HmacKeyId(key);
        }

        public static HmacKeyId FromString(string value) {
            return (HmacKeyId) value;
        }
        
        public static implicit operator string(HmacKeyId id) {
            return id.ToString();
        }

        public override string ToString() {
            return $"{Prefix}{Key ?? string.Empty}";
        }
    }
}