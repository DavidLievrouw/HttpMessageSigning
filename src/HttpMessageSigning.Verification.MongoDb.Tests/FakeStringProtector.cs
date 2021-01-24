using System;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class FakeStringProtector : IStringProtector {
        private Exception _ex;
        private const string Suffix = "_protected";

        public string Protect(string plainText) {
            return plainText + Suffix;
        }

        public string Unprotect(string cipherText) {
            if (_ex != null) throw _ex;
            
            return cipherText.EndsWith(Suffix)
                ? cipherText.Substring(0, cipherText.Length - Suffix.Length)
                : cipherText;
        }

        public void Throw(Exception ex) {
            _ex = ex;
        }
    }
}