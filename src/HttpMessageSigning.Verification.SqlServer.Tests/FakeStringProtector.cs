using System;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class FakeStringProtector : IStringProtector {
        private Exception _ex;
        private const string Suffix = "_protected";

        public string Protect(string plainText) {
            return plainText + Suffix;
        }

        public string Unprotect(string cipherText) {
            if (_ex != null) throw _ex;
            
#if NET6_0_OR_GREATER
            return cipherText.EndsWith(Suffix)
                ? cipherText[..^Suffix.Length]
                : cipherText;
#else
            return cipherText.EndsWith(Suffix)
                ? cipherText.Substring(0, cipherText.Length - Suffix.Length)
                : cipherText;
#endif
        }

        public void Throw(Exception ex) {
            _ex = ex;
        }
    }
}