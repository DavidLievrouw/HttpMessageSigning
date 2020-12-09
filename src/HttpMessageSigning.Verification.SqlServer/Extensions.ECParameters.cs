using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public static partial class Extensions {
        internal static string ToXml(this ECParameters parameters) {
            // ReSharper disable once UseStringInterpolation
            return string.Format(
                "<ECDsaKeyValue><FriendlyName>{0}</FriendlyName><Q.X>{1}</Q.X><Q.Y>{2}</Q.Y></ECDsaKeyValue>",
                parameters.Curve.Oid.FriendlyName,
                Convert.ToBase64String(parameters.Q.X),
                Convert.ToBase64String(parameters.Q.Y));
        }
    }
}