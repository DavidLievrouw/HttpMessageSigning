using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Defines the available hooks during HTTP message signing.
    /// </summary>
    public class RequestSigningEvents {
        /// <summary>
        ///     Represents a delegate for the <see cref="RequestSigningEvents.OnRequestSigned" /> event.
        /// </summary>
        /// <param name="message">The <see cref="HttpRequestMessage" /> that was signed.</param>
        /// <param name="signingSettings">The signing settings that were used.</param>
        /// <param name="signature">The <see cref="Signature" /> that was created.</param>
        public delegate Task OnRequestSignedDelegate(HttpRequestMessage message, Signature signature, SigningSettings signingSettings);

        /// <summary>
        ///     Represents a delegate for the <see cref="RequestSigningEvents.OnRequestSigning" /> event.
        /// </summary>
        /// <param name="message">The <see cref="HttpRequestMessage" /> to sign.</param>
        /// <param name="signingSettings">The signing settings to use.</param>
        public delegate Task OnRequestSigningDelegate(HttpRequestMessage message, SigningSettings signingSettings);

        /// <summary>
        ///     Represents a delegate for the <see cref="RequestSigningEvents.OnSignatureCreated" /> event.
        /// </summary>
        /// <param name="message">The <see cref="HttpRequestMessage" /> to sign.</param>
        /// <param name="signingSettings">The signing settings to use.</param>
        /// <param name="signature">The <see cref="Signature" /> that was created.</param>
        public delegate Task OnSignatureCreatedDelegate(HttpRequestMessage message, Signature signature, SigningSettings signingSettings);

        /// <summary>
        ///     Represents a delegate for the <see cref="RequestSigningEvents.OnSigningStringComposed" /> event.
        /// </summary>
        /// <param name="message">The <see cref="HttpRequestMessage" /> to sign.</param>
        /// <param name="signingString">The composed signing string.</param>
        public delegate Task OnSigningStringComposedDelegate(HttpRequestMessage message, ref string signingString);

        /// <summary>
        ///     Gets or sets the action to invoke just before signing a request.
        /// </summary>
        public OnRequestSigningDelegate OnRequestSigning { get; set; } = (requestToSign, signingSettings) => Task.CompletedTask;

        /// <summary>
        ///     Gets or sets the action to invoke when the signing string has been composed.
        /// </summary>
        public OnSigningStringComposedDelegate OnSigningStringComposed { get; set; } = (HttpRequestMessage requestToSign, ref string signingString) => Task.CompletedTask;

        /// <summary>
        ///     Gets or sets the action to invoke when the request has been signed.
        /// </summary>
        public OnSignatureCreatedDelegate OnSignatureCreated { get; set; } = (signedRequest, signature, signingSettings) => Task.CompletedTask;

        /// <summary>
        ///     Gets or sets the action to invoke when the request has been signed.
        /// </summary>
        public OnRequestSignedDelegate OnRequestSigned { get; set; } = (signedRequest, signature, signingSettings) => Task.CompletedTask;
    }
}