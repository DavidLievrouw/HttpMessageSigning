using System;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    /// Factory that is able to create a <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>, based on the specified settings.
    /// </summary>
    public interface IRequestSignerFactory : IDisposable {
        /// <summary>
        /// Create a new <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>, based on the specified settings.
        /// </summary>
        /// <param name="keyId">The identifier that is associated with the client to sign the request for.</param>
        /// <param name="signingSettings">The settings that specify how to sign requests.</param>
        /// <returns>A new <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>.</returns>
        /// <remarks>The specified <see cref="Dalion.HttpMessageSigning.SigningSettings"/> are validated. When invalid, a new <see cref="ValidationException"/> is thrown.</remarks>
        IRequestSigner Create(KeyId keyId, SigningSettings signingSettings);

        /// <summary>
        /// Gets the <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>, that is associated with the specified <see cref="Dalion.HttpMessageSigning.KeyId"/>.
        /// </summary>
        /// <param name="keyId">The identifier that is associated with the client to sign the request for.</param>
        /// <returns>A new <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>.</returns>
        /// <remarks>The specified <see cref="Dalion.HttpMessageSigning.SigningSettings"/> are validated. When invalid, a new <see cref="ValidationException"/> is thrown.</remarks>
        IRequestSigner CreateFor(KeyId keyId);
    }
}