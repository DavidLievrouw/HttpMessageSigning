using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Utils {
    internal class SymmetricDataProtector : IDataProtector {
        private const int KeySize = 128;
        private const int DerivationIterations = 1000;

        private readonly byte[] _key;

        public SymmetricDataProtector(byte[] key) {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(key));
            _key = key;
        }

#if NET6_0_OR_GREATER
        public byte[] Protect(ReadOnlySpan<byte> data) {
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            using var password = new Rfc2898DeriveBytes(_key, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA1);
            var keyBytes = password.GetBytes(KeySize / 8);
            using var symmetricKey = Aes.Create();
            symmetricKey.BlockSize = 128;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;
            using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(data.ToArray(), 0, data.Length);
            cryptoStream.FlushFinalBlock();
            var cipherTextBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(saltStringBytes.Length + ivStringBytes.Length + (int)memoryStream.Length);
            try {
                Buffer.BlockCopy(saltStringBytes, 0, cipherTextBytes, 0, saltStringBytes.Length);
                Buffer.BlockCopy(ivStringBytes, 0, cipherTextBytes, saltStringBytes.Length, ivStringBytes.Length);
                Buffer.BlockCopy(memoryStream.ToArray(), 0, cipherTextBytes, saltStringBytes.Length + ivStringBytes.Length, (int)memoryStream.Length);
                return cipherTextBytes.Take(saltStringBytes.Length + ivStringBytes.Length + (int)memoryStream.Length).ToArray();
            }
            finally {
                System.Buffers.ArrayPool<byte>.Shared.Return(cipherTextBytes);
            }
        }

        public byte[] Unprotect(ReadOnlySpan<byte> cipher) {
            var saltStringBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(KeySize / 8);
            var ivStringBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(KeySize / 8);
            var cipherTextBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(cipher.Length - KeySize / 8 * 2);

            try {
                var cipherArray = cipher.ToArray();
                Buffer.BlockCopy(cipherArray, 0, saltStringBytes, 0, KeySize / 8);
                Buffer.BlockCopy(cipherArray, KeySize / 8, ivStringBytes, 0, KeySize / 8);
                Buffer.BlockCopy(cipherArray, KeySize / 8 * 2, cipherTextBytes, 0, cipher.Length - KeySize / 8 * 2);

                using var password = new Rfc2898DeriveBytes(_key, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA1);
                var keyBytes = password.GetBytes(KeySize / 8);
                using var symmetricKey = Aes.Create();
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes.AsSpan(0, KeySize / 8).ToArray());
                using var memoryStream = new MemoryStream(cipherTextBytes, 0, cipher.Length - KeySize / 8 * 2);
                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                var decryptedData = System.Buffers.ArrayPool<byte>.Shared.Rent(cipher.Length - KeySize / 8 * 2);
                try {
                    int bytesRead, totalBytesRead = 0;
                    while ((bytesRead = cryptoStream.Read(decryptedData, totalBytesRead, decryptedData.Length - totalBytesRead)) > 0) {
                        totalBytesRead += bytesRead;
                    }

                    return decryptedData.AsSpan(0, totalBytesRead).ToArray();
                }
                finally {
                    System.Buffers.ArrayPool<byte>.Shared.Return(decryptedData);
                }
            }
            finally {
                System.Buffers.ArrayPool<byte>.Shared.Return(saltStringBytes);
                System.Buffers.ArrayPool<byte>.Shared.Return(ivStringBytes);
                System.Buffers.ArrayPool<byte>.Shared.Return(cipherTextBytes);
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy() {
            var randomBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(16);
            try {
                using var rngCsp = RandomNumberGenerator.Create();
                rngCsp.GetBytes(randomBytes, 0, 16);
                return randomBytes.Take(16).ToArray();
            }
            finally {
                System.Buffers.ArrayPool<byte>.Shared.Return(randomBytes);
            }
        }
#else
        public byte[] Protect(byte[] data) {
            data = data ?? Array.Empty<byte>();
            
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            using (var password = new Rfc2898DeriveBytes(_key, saltStringBytes, DerivationIterations)) {
                var keyBytes = password.GetBytes(KeySize / 8);
                using (var symmetricKey = Aes.Create()) {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream()) {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                                cryptoStream.Write(data, 0, data.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(saltStringBytes.Length + ivStringBytes.Length + (int)memoryStream.Length);
                                try {
                                    Buffer.BlockCopy(saltStringBytes, 0, cipherTextBytes, 0, saltStringBytes.Length);
                                    Buffer.BlockCopy(ivStringBytes, 0, cipherTextBytes, saltStringBytes.Length, ivStringBytes.Length);
                                    Buffer.BlockCopy(memoryStream.ToArray(), 0, cipherTextBytes, saltStringBytes.Length + ivStringBytes.Length, (int)memoryStream.Length);
                                    return cipherTextBytes.Take(saltStringBytes.Length + ivStringBytes.Length + (int)memoryStream.Length).ToArray();
                                }
                                finally {
                                    System.Buffers.ArrayPool<byte>.Shared.Return(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }
        }

        public byte[] Unprotect(byte[] cipher) {
            cipher = cipher ?? Array.Empty<byte>();
            
            var saltStringBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(KeySize / 8);
            var ivStringBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(KeySize / 8);
            var cipherTextBytes = System.Buffers.ArrayPool<byte>.Shared.Rent(cipher.Length - KeySize / 8 * 2);

            try {
                var cipherArray = cipher.ToArray();
                Buffer.BlockCopy(cipherArray, 0, saltStringBytes, 0, KeySize / 8);
                Buffer.BlockCopy(cipherArray, KeySize / 8, ivStringBytes, 0, KeySize / 8);
                Buffer.BlockCopy(cipherArray, KeySize / 8 * 2, cipherTextBytes, 0, cipher.Length - KeySize / 8 * 2);

                using (var password = new Rfc2898DeriveBytes(_key, saltStringBytes, DerivationIterations)) {
                    var keyBytes = password.GetBytes(KeySize / 8);
                    using (var symmetricKey = Aes.Create()) {
                        symmetricKey.BlockSize = 128;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes.Take(KeySize / 8).ToArray())) {
                            using (var memoryStream = new MemoryStream(cipherTextBytes, 0, cipher.Length - KeySize / 8 * 2)) {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                                    var decryptedData = System.Buffers.ArrayPool<byte>.Shared.Rent(cipher.Length - KeySize / 8 * 2);
                                    try {
                                        int bytesRead, totalBytesRead = 0;
                                        while ((bytesRead = cryptoStream.Read(decryptedData, totalBytesRead, decryptedData.Length - totalBytesRead)) > 0) {
                                            totalBytesRead += bytesRead;
                                        }

                                        return decryptedData.Take(totalBytesRead).ToArray();
                                    }
                                    finally {
                                        System.Buffers.ArrayPool<byte>.Shared.Return(decryptedData);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally {
                System.Buffers.ArrayPool<byte>.Shared.Return(saltStringBytes);
                System.Buffers.ArrayPool<byte>.Shared.Return(ivStringBytes);
                System.Buffers.ArrayPool<byte>.Shared.Return(cipherTextBytes);
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy() {
            var randomBytes = new byte[16];
            using (var rngCsp = RandomNumberGenerator.Create()) {
                rngCsp.GetBytes(randomBytes);
            }

            return randomBytes;
        }
#endif
    }
}