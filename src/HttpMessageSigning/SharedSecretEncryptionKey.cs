using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an encryption key for shared secrets of symmetric encryption algorithms.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public readonly struct SharedSecretEncryptionKey : IEquatable<SharedSecretEncryptionKey> {
        /// <summary>
        ///     Creates a new <see cref="SharedSecretEncryptionKey" />.
        /// </summary>
        /// <param name="value">The string representation of this instance.</param>
        public SharedSecretEncryptionKey(string value) {
            Value = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets the string representation of this instance.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Represents an empty <see cref="SharedSecretEncryptionKey" />.
        /// </summary>
        public static SharedSecretEncryptionKey Empty { get; } = new SharedSecretEncryptionKey(string.Empty);

        /// <inheritdoc />
        public bool Equals(SharedSecretEncryptionKey other) {
            return (Value ?? string.Empty) == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            return obj is SharedSecretEncryptionKey other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return (Value ?? string.Empty).GetHashCode();
        }

        /// <summary>
        ///     The equality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are equal, otherwise false.</returns>
        public static bool operator ==(SharedSecretEncryptionKey left, SharedSecretEncryptionKey right) {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are not equal, otherwise false.</returns>
        public static bool operator !=(SharedSecretEncryptionKey left, SharedSecretEncryptionKey right) {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Try to parse the specified string to a <see cref="SharedSecretEncryptionKey" />.
        /// </summary>
        /// <param name="value">The string representation to parse.</param>
        /// <param name="parsed">This will contain the parsed <see cref="SharedSecretEncryptionKey" /> when parsing succeeded, otherwise <see cref="SharedSecretEncryptionKey.Empty" />.</param>
        /// <returns>True if parsing succeeded, otherwise false.</returns>
        public static bool TryParse(string value, out SharedSecretEncryptionKey parsed) {
            parsed = new SharedSecretEncryptionKey(value);
            return true;
        }

        /// <summary>
        ///     An implicit conversion operator for this type from string.
        /// </summary>
        public static implicit operator SharedSecretEncryptionKey(string value) {
            if (!TryParse(value, out var sharedSecretEncryptionKey)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a {nameof(SharedSecretEncryptionKey)}.");
            }

            return sharedSecretEncryptionKey;
        }

        /// <summary>
        ///     An implicit conversion operator for this type to string.
        /// </summary>
        public static implicit operator string(SharedSecretEncryptionKey key) {
            return key.ToString();
        }

        /// <inheritdoc />
        public override string ToString() {
            return Value ?? string.Empty;
        }
    }
}