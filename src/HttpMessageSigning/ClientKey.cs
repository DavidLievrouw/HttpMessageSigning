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
        /// <remarks>In the case of symmetric keys, this value represents the shared key. In case of an asymmetric key, this value represents the private key of the client.</remarks>
        public Secret Secret { get; set; }
        
        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (Id == null) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Id)}.");
            if (Id == KeyId.Empty) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Id)}.");
            if (Secret == null) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Secret)}.");
            if (Secret == Secret.Empty) throw new ValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Secret)}.");
        }
    }
}