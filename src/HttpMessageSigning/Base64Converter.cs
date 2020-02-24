using System;

namespace Dalion.HttpMessageSigning {
    internal class Base64Converter : IBase64Converter {
        public byte[] FromBase64(string base64) {
            if (base64 == null) throw new ArgumentNullException(nameof(base64));
            return Convert.FromBase64String(base64);
        }

        public string ToBase64(byte[] bytes) {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            return Convert.ToBase64String(bytes);
        }
    }
}