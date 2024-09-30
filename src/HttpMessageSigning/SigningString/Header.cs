using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    internal struct Header : IEquatable<Header> {
        public static Header Empty = new Header(string.Empty, Array.Empty<string>());

        private string _stringRepresentation;

        public Header(string name, params string[] values) {
            if (values == null) values = Array.Empty<string>();
            Name = name?.ToLower() ?? throw new ArgumentNullException(nameof(name));
            if (name == string.Empty && values.Any()) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Values = values;
            _stringRepresentation = null;
        }

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
                var separatorIndex = value.IndexOf(": ", StringComparison.Ordinal);
                if (separatorIndex < 0 || separatorIndex >= value.Length - 1) {
                    nameAndValues.Add(value);
                }
                else {
#if NET6_0_OR_GREATER
                    nameAndValues.Add(value[..separatorIndex]);
                    nameAndValues.Add(value[(separatorIndex + 1)..]);
#else
                    nameAndValues.Add(value.Substring(0, separatorIndex));
                    nameAndValues.Add(value.Substring(separatorIndex + 1));
#endif
                }
            }

            if (nameAndValues.Count != 2) return false;
            var name = nameAndValues[0];
            if (string.IsNullOrEmpty(name.Trim())) return false;
            var values = nameAndValues[1]
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();

            parsed = new Header(name, values);

            return true;
        }

        public static Header Parse(string header) {
            return (Header)header;
        }

        public static implicit operator string(Header header) {
            return header.ToString();
        }

        public override string ToString() {
            if (_stringRepresentation == null) {
                var sb = new StringBuilder();
                sb.Append(Name);
                sb.Append(": ");
                sb.Append(string.Join(", ", Values));
                _stringRepresentation = sb.ToString();
            }

            return _stringRepresentation;
        }
    }
}