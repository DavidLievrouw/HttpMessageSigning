using System;

namespace Dalion.HttpMessageSigning.Utils {
    internal interface IDataProtector {
#if NET8_0_OR_GREATER
        byte[] Protect(ReadOnlySpan<byte> data);
        byte[] Unprotect(ReadOnlySpan<byte> cipher);
#else
        byte[] Protect(byte[] data);
        byte[] Unprotect(byte[] cipher);
#endif
    }
}