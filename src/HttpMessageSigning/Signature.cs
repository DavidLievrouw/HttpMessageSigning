using System;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents a signature that is received from a client.
    /// </summary>
    public class Signature : IValidatable {
        /// <summary>
        /// REQUIRED: Gets or sets the entity that the server can use to look up the component they need to validate the signature.
        /// </summary>
        public KeyId KeyId { get; set; }
        
        /// <summary>
        /// OPTIONAL: Gets or sets the algorithm that needs to be used to validate the hash value.
        /// </summary>
        public string Algorithm { get; set; }
        
        /// <summary>
        /// OPTIONAL: Gets or sets the time at which the signature was created.
        /// </summary>
        public DateTimeOffset? Created { get; set; }
        
        /// <summary>
        /// OPTIONAL: Gets or sets the timespan after which the signature must be considered expired.
        /// </summary>
        public DateTimeOffset? Expires { get; set; }
        
        /// <summary>
        /// OPTIONAL: Gets or sets the ordered list of names of request headers to include when validating the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be considered, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; }
        
        /// <summary>
        /// Gets or sets the signature string to be validated by the server.
        /// </summary>
        public string String { get; set; }
                
        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (KeyId == KeyId.Empty) throw new ValidationException($"The {nameof(Signature)} do not specify a valid {nameof(KeyId)}.");
            if (string.IsNullOrEmpty(String)) throw new ValidationException($"The {nameof(Signature)} do not specify a valid signature {nameof(String)}.");
        }
    }
}