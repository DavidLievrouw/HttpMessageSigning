using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an algorithm that can be used to create a signature of specified string content.
    /// </summary>
    public interface ISignatureAlgorithm : IDisposable {
        /// <summary>
        ///     Gets the descriptive name of the signature algorithm.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the name of the hash algorithm to use when hashing the content.
        /// </summary>
        HashAlgorithmName HashAlgorithm { get; }

        /// <summary>
        ///     Compute the hash of the specified content.
        /// </summary>
        /// <param name="contentToSign">The string content to compute the signature for.</param>
        /// <returns>The binary representation of the hashed content.</returns>
        byte[] ComputeHash(string contentToSign);

        /// <summary>
        ///     Verify that the specified signature is a match for the specified string content.
        /// </summary>
        /// <param name="contentToSign">The string content to verify the signature for.</param>
        /// <param name="signature">The hashed signature that should match.</param>
        /// <returns>True if the specified signature matches the expected value, otherwise false.</returns>
        bool VerifySignature(string contentToSign, byte[] signature);
    }
}