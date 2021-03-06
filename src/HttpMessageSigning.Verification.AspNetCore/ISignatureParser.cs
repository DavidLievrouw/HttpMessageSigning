using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    ///     Supports parsing the specified <see cref="HttpRequest" /> to a <see cref="Signature" />.
    /// </summary>
    public interface ISignatureParser {
        /// <summary>
        ///     Parse the specified <see cref="HttpRequest" /> to a <see cref="Signature" />.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest" /> to parse.</param>
        /// <param name="options">The configured authentication options.</param>
        /// <returns>The result of the parsing, containing the parsed signature if successful, or the cause of failure.</returns>
        SignatureParsingResult Parse(HttpRequest request, SignedRequestAuthenticationOptions options);
    }
}