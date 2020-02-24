using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct SelfContainedKeyId : IEquatable<SelfContainedKeyId>, IKeyId {
        private static readonly Regex KeyIdRegEx = new Regex(@"^sig=(?<sig>[A-z0-9-_]+), hash=(?<hash>[A-z0-9-_]+), key=(?<key>[a-zA-Z0-9+/]+={0,2})$", RegexOptions.Compiled);

        public SelfContainedKeyId(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, string key) {
            SignatureAlgorithm = signatureAlgorithm;
            HashAlgorithm = hashAlgorithm;
            Value = key ?? string.Empty;
        }

        public HashAlgorithm HashAlgorithm { get; }
        
        public SignatureAlgorithm SignatureAlgorithm { get; }
        
        public string Value { get; }

        public bool Equals(SelfContainedKeyId other) {
            return HashAlgorithm == other.HashAlgorithm && SignatureAlgorithm == other.SignatureAlgorithm && Value == other.Value;
        }

        public override bool Equals(object obj) {
            return obj is SelfContainedKeyId other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) HashAlgorithm;
                hashCode = (hashCode * 397) ^ (int) SignatureAlgorithm;
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SelfContainedKeyId left, SelfContainedKeyId right) {
            return left.Equals(right);
        }

        public static bool operator !=(SelfContainedKeyId left, SelfContainedKeyId right) {
            return !left.Equals(right);
        }

        public static explicit operator SelfContainedKeyId(string value) {
            if (!TryParse(value, out var keyId)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a key id.");
            }
            return keyId;
        }

        public static bool TryParse(string value, out SelfContainedKeyId parsed) {
            parsed = new SelfContainedKeyId();
            
            if (string.IsNullOrEmpty(value)) return false;
            
            var match = KeyIdRegEx.Match(value);
            if (!match.Success) return false;
            if (!Enum.TryParse(match.Groups["sig"]?.Value, true, out SignatureAlgorithm sig)) return false;
            if (!Enum.TryParse(match.Groups["hash"]?.Value, true, out HashAlgorithm hash)) return false;
            
            parsed = new SelfContainedKeyId(sig, hash, match.Groups["key"].Value);

            return true;
        }
        
        public static SelfContainedKeyId Parse(string value) {
            return (SelfContainedKeyId) value;
        }
        
        public static implicit operator string(SelfContainedKeyId id) {
            return id.ToString();
        }

        public override string ToString() {
            return $"sig={SignatureAlgorithm.ToString().ToLowerInvariant()}, hash={HashAlgorithm.ToString().ToLowerInvariant()}, key={Value ?? string.Empty}";
        }
    }
}