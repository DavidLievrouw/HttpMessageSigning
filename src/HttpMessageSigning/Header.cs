using System;
using System.Diagnostics;
using System.Linq;

namespace Dalion.HttpMessageSigning {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public struct Header : IEquatable<Header> {
        private Header(string name, string[] values) {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }
        
        public Header(string name, string value, params string[] values) {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (values == null) values = Array.Empty<string>();
            Name = name;
            Values = new[] {value}.Concat(values).ToArray();
        }
        
        public string Name { get; }

        public string[] Values { get; }

        public bool Equals(Header other) {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            return obj is Header other && Equals(other);
        }

        public override int GetHashCode() {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        }

        public static bool operator ==(Header left, Header right) {
            return left.Equals(right);
        }

        public static bool operator !=(Header left, Header right) {
            return !left.Equals(right);
        }

        public static explicit operator Header(string header) {
            if (string.IsNullOrEmpty(header)) throw new ArgumentException("Value cannot be null or empty.", nameof(header));
            var nameAndValues = header.Split(new[]{": "}, StringSplitOptions.RemoveEmptyEntries);
            if (nameAndValues.Length != 2) throw new FormatException($"The specified value ({header}) is not a valid string representation of a header.");
            var name = nameAndValues[0];
            if (string.IsNullOrEmpty(name.Trim())) throw new FormatException($"The specified value ({header}) is not a valid string representation of a header.");
            var values = nameAndValues[1].Split(new[]{", "}, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
            if (values.Length < 1) throw new FormatException($"The specified value ({header}) is not a valid string representation of a header.");
            return new Header(name, values);
        }

        public static Header FromString(string header) {
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