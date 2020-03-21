namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an object that can self-validate its properties.
    /// </summary>
    public interface IValidatable {
        /// <summary>
        /// Validates the properties of this instance, and throws an <see cref="ValidationException"/> when any of them have invalid values.
        /// </summary>
        void Validate();
    }
}