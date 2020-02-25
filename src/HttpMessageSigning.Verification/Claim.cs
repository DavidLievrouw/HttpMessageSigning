using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a statement about an entity by an issuer.
    /// </summary>
    public class Claim {
        public Claim() { }
        
        public Claim(string type, string value) {
            if (string.IsNullOrEmpty(type)) throw new ArgumentException("Value cannot be null or empty.", nameof(type));
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets the type of the <see cref="Claim"/>.
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Gets the value of the <see cref="Claim"/>.
        /// </summary>
        public string Value { get; set; }
    }
}