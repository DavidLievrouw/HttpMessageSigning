using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    internal struct Header : IEquatable<Header> {
        public Header(string name, params string[] values) {
            if (values == null) values = Array.Empty<string>();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (name == string.Empty && values.Any()) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Values = values;
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
                throw new FormatException($"The specified value ({value}) is not a valid string representation of a header.");
            }
            return header;
        }

        public static bool TryParse(string value, out Header parsed) {
            parsed = Empty;
            
            if (string.IsNullOrEmpty(value)) return false;
            
            var nameAndValues = new List<string>();
            if (!string.IsNullOrEmpty(value)) {
                var separatorIndex = value.IndexOf(": ", StringComparison.InvariantCulture);
                if (separatorIndex < 0 || separatorIndex >= value.Length - 1) {
                    nameAndValues.Add(value);
                }
                else {
                    nameAndValues.Add(value.Substring(0, separatorIndex));
                    nameAndValues.Add(value.Substring(separatorIndex + 1));
                }
            }
            
            if (nameAndValues.Count != 2) return false;
            var name = nameAndValues[0];
            if (string.IsNullOrEmpty(name.Trim())) return false;
            var values = nameAndValues[1]
                .Split(new[]{", "}, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();
            
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