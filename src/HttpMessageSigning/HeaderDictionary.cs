using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     A dictionary that contains headers, identified by their names.
    /// </summary>
    public class HeaderDictionary : IEnumerable<KeyValuePair<string, StringValues>> {
        private readonly IDictionary<string, StringValues> _innerDictionary;

        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        public HeaderDictionary() {
            _innerDictionary = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="headers">The data to initialize this instance with.</param>
        public HeaderDictionary(IDictionary<string, StringValues> headers) {
            if (headers == null) headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            _innerDictionary = new Dictionary<string, StringValues>(headers, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Gets or sets the indexer property of this dictionary.
        /// </summary>
        /// <param name="key">The key to get or set the values for.</param>
        /// <remarks>If no header with the specified key is found, <see cref="StringValues.Empty" /> is returned.</remarks>
        public StringValues this[string key] {
            get => _innerDictionary.TryGetValue(key, out var values) ? values : StringValues.Empty;
            set => _innerDictionary[key] = value;
        }

        /// <summary>
        ///     Gets the number of items contained by this dictionary.
        /// </summary>
        public int Count => _innerDictionary.Count;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() {
            return _innerDictionary.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     Gets a value indicating whether this dictionary contains an item with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if the dictionary contains an item with the specified key, otherwise false.</returns>
        public bool Contains(string key) {
            return _innerDictionary.ContainsKey(key);
        }

        /// <summary>
        ///     Remove the item with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if an item was removed, otherwise false.</returns>
        public bool Remove(string key) {
            return _innerDictionary.Remove(key);
        }

        /// <summary>
        ///     Gets the values of the header associated with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The values of the header associated with the specified key.</returns>
        /// <remarks>If no header with the specified key is found, <see cref="StringValues.Empty" /> is returned.</remarks>
        public StringValues GetValues(string key) {
            return this[key];
        }

        /// <summary>
        ///     Clears all items from this dictionary.
        /// </summary>
        public void Clear() {
            _innerDictionary.Clear();
        }

        /// <summary>
        ///     Add an item to this dictionary.
        /// </summary>
        /// <param name="key">The key of the item to add.</param>
        /// <param name="values">The values of the item to add.</param>
        public void Add(string key, StringValues values) {
            _innerDictionary.Add(key, values);
        }

        /// <summary>
        ///     Tries to get the values of the header associated with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="values">This will contain the values of the header, if one is found, otherwise <see cref="StringValues.Empty" />.</param>
        /// <returns>True if an item with the specified key was found, otherwise false.</returns>
        public bool TryGetValues(string key, out StringValues values) {
            return _innerDictionary.TryGetValue(key, out values);
        }

        /// <summary>
        ///     Create a new <see cref="IDictionary" /> out of the contents of this dictionary.
        /// </summary>
        /// <returns>A new <see cref="IDictionary" /> with the contents of this dictionary.</returns>
        public IDictionary<string, StringValues> ToDictionary() {
            return new Dictionary<string, StringValues>(_innerDictionary, StringComparer.OrdinalIgnoreCase);
        }
    }
}