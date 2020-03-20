using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a value that the server can use to check nonce validity.
    /// </summary>
    public class Nonce {
        public Nonce(KeyId clientId, string value, DateTimeOffset expiration) {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be empty.", nameof(clientId));
            ClientId = clientId;
            Value = value;
            Expiration = expiration;
        }

        /// <summary>
        /// Gets or sets the identity of the client for which the nonce is used.
        /// </summary>
        public KeyId ClientId { get; set; }
        
        /// <summary>
        /// Gets or sets the nonce value.
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the nonce value can be reused for subsequent requests form the client.
        /// </summary>
        public DateTimeOffset Expiration { get; set; }
    }
}