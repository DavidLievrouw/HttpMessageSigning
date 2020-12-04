using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents an object that creates authenticated principal, after successful signature verification.
    /// </summary>
    public interface IClaimsPrincipalFactory {
        /// <summary>
        ///     Create an authenticated principal, using the settings of the specified <see cref="Client" />.
        /// </summary>
        /// <param name="client">The <see cref="Client" /> whose settings were used to verify the signature.</param>
        /// <returns>An authenticated <see cref="ClaimsPrincipal" />.</returns>
        ClaimsPrincipal CreateForClient(Client client);
    }
}