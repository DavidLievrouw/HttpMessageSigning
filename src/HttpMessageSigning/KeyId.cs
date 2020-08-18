using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an entity that the server can use to look up the component they need to verify the signature.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public readonly struct KeyId : IEquatable<KeyId> {
        /// <summary>
        ///     Creates a new <see cref="KeyId" />.
        /// </summary>
        /// <param name="value">The string representation of this instance.</param>
        public KeyId(string value) {
            Value = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets the string representation of this instance.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Represents an empty <see cref="KeyId" />.
        /// </summary>
        public static KeyId Empty { get; } = new KeyId(string.Empty);

        /// <inheritdoc />
        public bool Equals(KeyId other) {
            return (Value ?? string.Empty) == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            return obj is KeyId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return (Value ?? string.Empty).GetHashCode();
        }

        /// <summary>
        ///     The equality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are equal, otherwise false.</returns>
        public static bool operator ==(KeyId left, KeyId right) {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are not equal, otherwise false.</returns>
        public static bool operator !=(KeyId left, KeyId right) {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Try to parse the specified string to a <see cref="KeyId" />.
        /// </summary>
        /// <param name="value">The string representation to parse.</param>
        /// <param name="parsed">This will contain the parsed <see cref="KeyId" /> when parsing succeeded, otherwise <see cref="KeyId.Empty" />.</param>
        /// <returns>True if parsing succeeded, otherwise false.</returns>
        public static bool TryParse(string value, out KeyId parsed) {
            parsed = new KeyId(value);
            return true;
        }

        /// <summary>
        ///     An implicit conversion operator for this type from string.
        /// </summary>
        public static implicit operator KeyId(string value) {
            if (!TryParse(value, out var keyId)) {
                throw new FormatException($"The specified value ({value ?? "[null]"}) is not a valid string representation of a key id.");
            }

            return keyId;
        }

        /// <summary>
        ///     An implicit conversion operator for this type to string.
        /// </summary>
        public static implicit operator string(KeyId id) {
            return id.ToString();
        }

        /// <inheritdoc />
        public override string ToString() {
            return Value ?? string.Empty;
        }
    }
}