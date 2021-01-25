using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        /// <summary>
        ///     Serialize the specified <see cref="RSAParameters" /> to an XML string.
        /// </summary>
        /// <param name="parameters">The <see cref="RSAParameters" /> to serialize.</param>
        /// <returns>An XML string that represents the specified <see cref="RSAParameters" />.</returns>
        public static string ToXml(this RSAParameters parameters) {
            // ReSharper disable once UseStringInterpolation
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
                parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null,
                parameters.P != null ? Convert.ToBase64String(parameters.P) : null,
                parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null,
                parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null,
                parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null,
                parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null,
                parameters.D != null ? Convert.ToBase64String(parameters.D) : null);
        }
    }
}