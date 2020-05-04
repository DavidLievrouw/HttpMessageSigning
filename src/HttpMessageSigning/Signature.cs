using System;
using System.Linq;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents a signature that is received from a client.
    /// </summary>
    public class Signature : IValidatable, ICloneable {
        /// <summary>
        /// Represents the name of the default signature algorithm name to include in the signature.
        /// </summary>
        public const string DefaultSignatureAlgorithm = "hs2019";
        
        /// <summary>
        /// REQUIRED: Gets or sets the entity that the server can use to look up the component they need to verify the signature.
        /// </summary>
        public KeyId KeyId { get; set; }

        /// <summary>
        /// OPTIONAL: Gets or sets the algorithm that needs to be used to verify the signature.
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
        /// OPTIONAL: Gets or sets the ordered list of names of request headers to include when verifying the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be considered, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; }

        /// <summary>
        /// OPTIONAL: Gets or sets a unique string, that is part of the signature, to prevent replay attacks.
        /// </summary>
        /// <remarks>When null or empty, this value will not be part of the signature.</remarks>
        public string Nonce { get; set; }
        
        /// <summary>
        /// REQUIRED: Gets or sets the signature string to be verified by the server.
        /// </summary>
        public string String { get; set; }

        /// <summary>
        /// Validates the properties of this instance, and throws an <see cref="ValidationException"/> when any of them have invalid values.
        /// </summary>
        public void Validate() {
            if (KeyId == KeyId.Empty) throw new ValidationException($"The {nameof(Signature)} do not specify a valid {nameof(KeyId)}.");
            if (string.IsNullOrEmpty(String)) throw new ValidationException($"The {nameof(Signature)} do not specify a valid signature {nameof(String)}.");
            if (Headers == null) throw new ValidationException($"{nameof(Headers)} cannot be unspecified (null).");
            if (!Headers.Any()) throw new ValidationException($"{nameof(Headers)} cannot be unspecified empty.");
        }

        public object Clone() {
            return new Signature {
                KeyId = KeyId,
                Algorithm = Algorithm,
                Created = Created,
                Expires = Expires,
                Headers = (HeaderName[]) Headers?.Clone(),
                Nonce = Nonce,
                String = String
            };
        }
    }
}