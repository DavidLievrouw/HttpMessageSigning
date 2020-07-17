using System.Collections.Generic;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an object that can self-validate its properties.
    /// </summary>
    public interface IValidatable {
        /// <summary>
        ///     Validates the properties of this instance, and throws an <see cref="ValidationException" /> when any of them have invalid values.
        /// </summary>
        void Validate();

        /// <summary>
        ///     Validates the properties of this instance, and returns a list of validation errors.
        /// </summary>
        /// <returns>The list with validation errors, or an empty list, when this instance is valid.</returns>
        IEnumerable<ValidationError> GetValidationErrors();
    }
}