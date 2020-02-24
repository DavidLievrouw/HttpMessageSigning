namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    /// Factory that is able to create a <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>, based on the specified settings.
    /// </summary>
    public interface IRequestSignerFactory {
        /// <summary>
        /// Create a new <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/>, based on the specified settings.
        /// </summary>
        /// <param name="signingSettings">The settings that specify how to sign requests.</param>
        /// <returns>A new <see cref="Dalion.HttpMessageSigning.Signing.IRequestSigner"/></returns>
        /// <remarks>The specified a new <see cref="Dalion.HttpMessageSigning.SigningSettings"/> are validated. When invalid, a a new <see cref="Dalion.HttpMessageSigning.HttpMessageSigningValidationException"/> is thrown.</remarks>
        IRequestSigner Create(SigningSettings signingSettings);
    }
}