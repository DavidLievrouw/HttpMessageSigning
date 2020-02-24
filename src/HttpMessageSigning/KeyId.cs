using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct KeyId : IEquatable<KeyId>, IKeyId {
        private static readonly Regex KeyIdRegEx = new Regex(@"^sig=(?<sig>[A-z0-9-_]+), hash=(?<hash>[A-z0-9-_]+), key=(?<key>[a-zA-Z0-9+/]+={0,2})$", RegexOptions.Compiled);

        public KeyId(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, string key) {
            SignatureAlgorithm = signatureAlgorithm;
            HashAlgorithm = hashAlgorithm;
            Key = key ?? string.Empty;
        }

        public HashAlgorithm HashAlgorithm { get; }
        
        public SignatureAlgorithm SignatureAlgorithm { get; }
        
        public string Key { get; }

        public bool Equals(KeyId other) {
            return HashAlgorithm == other.HashAlgorithm && SignatureAlgorithm == other.SignatureAlgorithm && Key == other.Key;
        }

        public override bool Equals(object obj) {
            return obj is KeyId other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) HashAlgorithm;
                hashCode = (hashCode * 397) ^ (int) SignatureAlgorithm;
                hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(KeyId left, KeyId right) {
            return left.Equals(right);
        }

        public static bool operator !=(KeyId left, KeyId right) {
            return !left.Equals(right);
        }

        public static explicit operator KeyId(string value) {
            if (!TryParse(value, out var keyId)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a key id.");
            }
            return keyId;
        }

        public static bool TryParse(string value, out KeyId parsed) {
            parsed = new KeyId();
            
            if (string.IsNullOrEmpty(value)) return false;
            
            var match = KeyIdRegEx.Match(value);
            if (!match.Success) return false;
            if (!Enum.TryParse(match.Groups["sig"]?.Value, true, out SignatureAlgorithm sig)) return false;
            if (!Enum.TryParse(match.Groups["hash"]?.Value, true, out HashAlgorithm hash)) return false;
            
            parsed = new KeyId(sig, hash, match.Groups["key"].Value);

            return true;
        }
        
        public static KeyId Parse(string value) {
            return (KeyId) value;
        }
        
        public static implicit operator string(KeyId id) {
            return id.ToString();
        }

        public override string ToString() {
            return $"sig={SignatureAlgorithm.ToString().ToLowerInvariant()}, hash={HashAlgorithm.ToString().ToLowerInvariant()}, key={Key ?? string.Empty}";
        }
    }
}