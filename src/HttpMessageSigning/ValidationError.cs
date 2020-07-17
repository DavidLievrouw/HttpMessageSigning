using System;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents a validation error.
    /// </summary>
    public class ValidationError {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="propertyName">The name of the property for which validation failed.</param>
        /// <param name="message">The message associated with the error.</param>
        public ValidationError(string propertyName, string message) {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Value cannot be null or empty.", nameof(propertyName));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Value cannot be null or empty.", nameof(message));
            PropertyName = propertyName;
            Message = message;
        }

        /// <summary>
        ///     Gets the name of the property for which validation failed.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        ///     Gets the message associated with the error.
        /// </summary>
        public string Message { get; }
    }
}