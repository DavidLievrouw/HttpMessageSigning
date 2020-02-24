namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an entity that the server can use to look up the component they need to validate the signature.
    /// </summary>
    public interface IKeyId {
        /// <summary>
        /// Gets the algorithm that is used to create the hash value.
        /// </summary>
        HashAlgorithm HashAlgorithm { get; }
        
        /// <summary>
        /// Gets the keyed hash algorithm that is used to create the hash value.
        /// </summary>
        SignatureAlgorithm SignatureAlgorithm { get; }
        
        /// <summary>
        /// Gets the opaque key that is used by the signature algorithm, by which the sender can be uniquely identified.
        /// </summary>
        string Key { get; }
    }
}