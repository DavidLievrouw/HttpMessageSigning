using System.Diagnostics.CodeAnalysis;

namespace Dalion.HttpMessageSigning {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum HashAlgorithm {
        None,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }
}