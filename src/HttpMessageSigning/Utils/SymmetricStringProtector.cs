using System;
using System.Text;

namespace Dalion.HttpMessageSigning.Utils {
    internal class SymmetricStringProtector : IStringProtector {
        private readonly SymmetricDataProtector _symmetricDataProtector;

        public SymmetricStringProtector(string secret) {
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException("Value cannot be null or empty.", nameof(secret));
            _symmetricDataProtector = new SymmetricDataProtector(Encoding.UTF8.GetBytes(secret));
        }

        public string Protect(string plain) {
            if (plain == null) throw new ArgumentNullException(nameof(plain));

            var protectedData = _symmetricDataProtector.Protect(Encoding.UTF8.GetBytes(plain));
            return Convert.ToBase64String(protectedData);
        }

        public string Unprotect(string cipher) {
            if (cipher == null) throw new ArgumentNullException(nameof(cipher));

            var protectedData = Convert.FromBase64String(cipher);
            var unprotectedData = _symmetricDataProtector.Unprotect(protectedData);
            return Encoding.UTF8.GetString(unprotectedData);
        }
    }
}