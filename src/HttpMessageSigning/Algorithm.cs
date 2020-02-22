using System.Diagnostics.CodeAnalysis;

namespace Dalion.HttpMessageSigning {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Algorithm {
        hs2019,
        rsa_sha1,
        rsa_sha256,
        hmac_sha256,
        ecdsa_sha256
    }
}