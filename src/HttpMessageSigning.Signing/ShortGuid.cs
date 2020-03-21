using System;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    ///     Represents a globally unique identifier (GUID) with a shorter string value.
    /// </summary>
    /// <remarks>Based on http://www.singular.co.nz/2007/12/shortguid-a-shorter-and-url-friendly-guid-in-c-sharp/</remarks>
    [Serializable]
    internal struct ShortGuid : IEquatable<ShortGuid>, IEquatable<string> {
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        /// <summary>
        ///     Creates a ShortGuid from a base64 encoded string.
        /// </summary>
        /// <param name="value">The encoded guid as a base64 string.</param>
        public ShortGuid(string value) {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Guid = Decode(value);
        }

        /// <summary>
        ///     Creates a ShortGuid from a Guid.
        /// </summary>
        /// <param name="guid">The Guid to encode.</param>
        public ShortGuid(Guid guid) {
            Value = Encode(guid);
            Guid = guid;
        }

        /// <summary>
        ///     Gets the underlying Guid.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets the underlying base64 encoded string.
        /// </summary>
        public string Value { get; }

        public bool Equals(ShortGuid other) {
            return Equals((object) other);
        }

        public bool Equals(string other) {
            return Equals((object) other);
        }

        public override string ToString() {
            return Value;
        }

        public override bool Equals(object obj) {
            if (obj is ShortGuid) return Guid.Equals(((ShortGuid) obj).Guid);
            if (obj is Guid) return Guid.Equals((Guid) obj);
            if (obj is string) return Guid.Equals(((ShortGuid) (string) obj).Guid);
            return false;
        }

        public override int GetHashCode() {
            return Guid.GetHashCode();
        }

        /// <summary>
        ///     Initialises a new instance of the ShortGuid class.
        /// </summary>
        /// <returns>A new instance of the ShortGuid class.</returns>
        public static ShortGuid NewGuid() {
            return new ShortGuid(Guid.NewGuid());
        }

        private static string Encode(Guid guid) {
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        private static Guid Decode(string value) {
            value = value
                .Replace("_", "/")
                .Replace("-", "+");
            var buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        public static bool operator ==(ShortGuid x, ShortGuid y) {
            return x.Guid == y.Guid;
        }

        public static bool operator !=(ShortGuid x, Guid y) {
            return !(x == y);
        }

        public static bool operator ==(ShortGuid x, Guid y) {
            return x.Guid == y;
        }

        public static bool operator !=(ShortGuid x, ShortGuid y) {
            return !(x == y);
        }

        public static implicit operator string(ShortGuid shortGuid) {
            return shortGuid.Value;
        }

        public static implicit operator Guid(ShortGuid shortGuid) {
            return shortGuid.Guid;
        }

        public static implicit operator ShortGuid(string shortGuid) {
            return new ShortGuid(shortGuid);
        }

        public static implicit operator ShortGuid(Guid guid) {
            return new ShortGuid(guid);
        }
    }
}