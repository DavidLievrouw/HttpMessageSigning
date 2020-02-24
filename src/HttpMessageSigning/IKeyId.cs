namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an entity that the server can use to look up the component they need to validate the signature.
    /// </summary>
    public interface IKeyId {
        /// <summary>
        /// Gets the opaque key that is used by the signature algorithm, by which the sender can be uniquely identified.
        /// </summary>
        string Value { get; }
    }
}