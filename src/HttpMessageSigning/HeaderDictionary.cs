using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning {
    internal class HeaderDictionary : IEnumerable<KeyValuePair<string, StringValues>> {
        private readonly IDictionary<string, StringValues> _innerDictionary;

        public HeaderDictionary() {
            _innerDictionary = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public HeaderDictionary(IDictionary<string, StringValues> headers) {
            if (headers == null) headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            _innerDictionary = new Dictionary<string, StringValues>(headers, StringComparer.OrdinalIgnoreCase);
        }
        
        public StringValues this[string key]
        {
            get => _innerDictionary.TryGetValue(key, out var values) ? values : StringValues.Empty;
            set => _innerDictionary[key] = value;
        }

        public bool Contains(string key) {
            return _innerDictionary.ContainsKey(key);
        }
        
        public bool Remove(string key) {
            return _innerDictionary.Remove(key);
        }
        
        public StringValues GetValues(string key) {
            return this[key];
        }

        public void Clear() {
            _innerDictionary.Clear();
        }
        
        public void Add(string key, StringValues values) {
            _innerDictionary.Add(key, values);
        }

        public int Count => _innerDictionary.Count;

        public bool TryGetValues(string key, out StringValues values) {
            return _innerDictionary.TryGetValue(key, out values);
        }

        public IDictionary<string, StringValues> ToDictionary() {
            return new Dictionary<string, StringValues>(_innerDictionary, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() {
            return _innerDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}