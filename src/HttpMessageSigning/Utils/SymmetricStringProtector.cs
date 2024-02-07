using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning.Utils {
    internal class SymmetricStringProtector : IStringProtector {
        private const int KeySize = 128;
        private const int DerivationIterations = 1000;

        private readonly string _secret;

        public SymmetricStringProtector(string secret) {
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException("Value cannot be null or empty.", nameof(secret));
            _secret = secret;
        }

        public string Protect(string plainText) {
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
#if NET8_0_OR_GREATER
            using (var password = new Rfc2898DeriveBytes(_secret, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA1)) {
#else
            using (var password = new Rfc2898DeriveBytes(_secret, saltStringBytes, DerivationIterations)) {
#endif
                var keyBytes = password.GetBytes(KeySize / 8);
                using (var symmetricKey = Aes.Create()) {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream()) {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public string Unprotect(string cipherText) {
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8 * 2).Take(cipherTextBytesWithSaltAndIv.Length - KeySize / 8 * 2).ToArray();
#if NET8_0_OR_GREATER
            using (var password = new Rfc2898DeriveBytes(_secret, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA1)) {
#else
            using (var password = new Rfc2898DeriveBytes(_secret, saltStringBytes, DerivationIterations)) {
#endif
                var keyBytes = password.GetBytes(KeySize / 8);
                using (var symmetricKey = Aes.Create()) {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream(cipherTextBytes)) {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                                using (var plainTextReader = new StreamReader(cryptoStream)) {
                                    return plainTextReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy() {
            var randomBytes = new byte[16];
            using (var rngCsp = RandomNumberGenerator.Create()) {
                rngCsp.GetBytes(randomBytes);
            }

            return randomBytes;
        }
    }
}