namespace Dalion.HttpMessageSigning.Utils {
    /// <summary>
    ///     Represents an object that can create instances of <see cref="IStringProtector" />.
    /// </summary>
    public interface IStringProtectorFactory {
        /// <summary>
        ///     Create a new <see cref="SymmetricStringProtector" />.
        /// </summary>
        /// <param name="secret">The secret <see cref="string" /> of the <see cref="SymmetricStringProtector" />.</param>
        /// <returns>A new <see cref="SymmetricStringProtector" /></returns>
        IStringProtector CreateSymmetric(string secret);
    }
}