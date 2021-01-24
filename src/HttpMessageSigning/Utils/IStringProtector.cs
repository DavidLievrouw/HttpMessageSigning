namespace Dalion.HttpMessageSigning.Utils {
    /// <summary>
    ///     Represents an object that can protect a given <see cref="string" />.
    /// </summary>
    public interface IStringProtector {
        /// <summary>
        ///     Protect the given plain-text <see cref="string" />.
        /// </summary>
        /// <param name="plainText">The plain-text <see cref="string" /> to protect.</param>
        /// <returns>The protected plain-text <see cref="string" />.</returns>
        string Protect(string plainText);

        /// <summary>
        ///     Undo the protection on the given cypher <see cref="string" />.
        /// </summary>
        /// <param name="cipherText">The cypher <see cref="string" /> to undo protection for.</param>
        /// <returns>The plain-text <see cref="string" />.</returns>
        string Unprotect(string cipherText);
    }
}