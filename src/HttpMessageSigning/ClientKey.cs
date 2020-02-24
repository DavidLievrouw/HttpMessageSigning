namespace Dalion.HttpMessageSigning {
    public class ClientKey : IValidatable {
        public KeyId Id { get; set; }
        public Secret Secret { get; set; }
        
        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (Id == null) throw new HttpMessageSigningValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Id)}.");
            if (Id == KeyId.Empty) throw new HttpMessageSigningValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Id)}.");
            if (Secret == null) throw new HttpMessageSigningValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Secret)}.");
            if (Secret == Secret.Empty) throw new HttpMessageSigningValidationException($"The {nameof(ClientKey)} do not specify a valid {nameof(Secret)}.");
        }
    }
}