using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents a service that can verify a signature, using the settings of the specified <see cref="Client" />.
    /// </summary>
    public interface ISignatureVerifier {
        /// <summary>
        ///     Verify the request signature, using the settings of the specified <see cref="Client" />.
        /// </summary>
        /// <param name="signedRequest">The request and its signature to verify.</param>
        /// <param name="client">The <see cref="Client" /> whose settings will be used to verify the signature.</param>
        /// <returns>The information about the verification failure, if verification failed, or a null reference, if verification succeeded.</returns>
        Task<SignatureVerificationFailure> VerifySignature(HttpRequestForVerification signedRequest, Client client);
    }
}