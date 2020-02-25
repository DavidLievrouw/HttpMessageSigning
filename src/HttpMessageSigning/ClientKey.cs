namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents the identifier and the secret for the client that signs a request.
    /// </summary>
    public class ClientKey : IValidatable {
        /// <summary>
        /// Gets or sets the identifier that the server can use to look up the component they need to validate the signature.
        /// </summary>
        public KeyId Id { get; set; }
        
        /// <summary>
        /// Gets or sets the secret that is used to sign a request, or to validate a signature.
        /// </summary>
        public Secret Secret { get; set; }
        
        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (Id == null) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Id)}.");
            if (Id == KeyId.Empty) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Id)}.");
            if (Secret == null) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Secret)}.");
            if (!(Secret is HMACSecret)) throw new ValidationException($"The {nameof(ClientKey)} specifies a currently unsupported {nameof(Secret)} type ({Secret.GetType().Name}).");
            if (Secret is HMACSecret hmacSecret && hmacSecret == HMACSecret.Empty) throw new ValidationException($"The {nameof(Secret)} of the {nameof(ClientKey)} cannot be an empty {nameof(HMACSecret)}.");
        }
    }
}