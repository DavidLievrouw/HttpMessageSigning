using System;
using System.Diagnostics;
using System.Linq;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct Header : IEquatable<Header> {
        private Header(string name, string[] values) {
            Name = name;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }
        
        public Header(string name, string value, params string[] values) {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (values == null) values = Array.Empty<string>();
            Name = name;
            Values = new[] {value}.Concat(values).Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v)).ToArray();
        }
        
        public static Header Empty = new Header(string.Empty, Array.Empty<string>());
        
        public string Name { get; }

        public string[] Values { get; }

        public bool Equals(Header other) {
            return string.Equals(Name ?? string.Empty, other.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            return obj is Header other && Equals(other);
        }

        public override int GetHashCode() {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name ?? string.Empty);
        }

        public static bool operator ==(Header left, Header right) {
            return left.Equals(right);
        }

        public static bool operator !=(Header left, Header right) {
            return !left.Equals(right);
        }

        public static explicit operator Header(string value) {
            if (string.IsNullOrEmpty(value)) return Empty;
            
            if (!TryParse(value, out var header)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a header.");
            }
            return header;
        }

        public static bool TryParse(string value, out Header parsed) {
            parsed = Empty;
            
            if (string.IsNullOrEmpty(value)) return false;
            
            var nameAndValues = value.Split(new[]{": "}, StringSplitOptions.None);
            if (nameAndValues.Length != 2) return false;
            var name = nameAndValues[0];
            if (string.IsNullOrEmpty(name.Trim())) return false;
            var values = nameAndValues[1].Split(new[]{", "}, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v)).ToArray();
            
            parsed =  new Header(name, values);

            return true;
        }
        
        public static Header Parse(string header) {
            return (Header) header;
        }
        
        public static implicit operator string(Header header) {
            return header.ToString();
        }

        public override string ToString() {
            var valuesString = string.Join(", ", Values.Where(v => !string.IsNullOrEmpty(v?.Trim())));
            return $"{Name.ToLowerInvariant()}: {valuesString}";
        }
    }
}