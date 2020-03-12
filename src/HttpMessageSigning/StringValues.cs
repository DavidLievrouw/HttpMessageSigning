using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dalion.HttpMessageSigning {
    public struct StringValues : IReadOnlyList<string>, IEquatable<StringValues> {
        public static readonly StringValues Empty = new StringValues(Array.Empty<string>());

        private readonly string[] _values;

        public StringValues(string value) {
            _values = value == null ? Array.Empty<string>() : new[] {value};
        }

        public StringValues(string[] values) {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool Equals(StringValues other) {
            if (Count != other.Count) {
                return false;
            }

            for (var i = 0; i < Count; i++) {
                if (this[i] != other[i]) {
                    return false;
                }
            }

            return true;
        }

        public IEnumerator<string> GetEnumerator() {
            return _values == null
                ? Enumerable.Empty<string>().GetEnumerator()
                : new List<string>(_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count => _values.Length;

        public string this[int index] {
            get => _values[index];
            set => _values[index] = value;
        }

        public static implicit operator StringValues(string value) {
            return new StringValues(value);
        }

        public static implicit operator StringValues(string[] values) {
            return new StringValues(values);
        }

        public static implicit operator string(StringValues values) {
            return values.GetStringValue();
        }

        public static implicit operator string[](StringValues value) {
            return value._values;
        }

        private string GetStringValue() {
            return string.Join(",", _values);
        }

        public override bool Equals(object obj) {
            return obj is StringValues other && Equals(other);
        }

        public override int GetHashCode() {
            return _values != null ? _values.GetHashCode() : 0;
        }

        public static bool operator ==(StringValues left, StringValues right) {
            return Equals(left, right);
        }

        public static bool operator !=(StringValues left, StringValues right) {
            return !Equals(left, right);
        }
    }
}