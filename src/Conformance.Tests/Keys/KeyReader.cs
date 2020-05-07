using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using PemUtils;

namespace Dalion.HttpMessageSigning.Keys {
    public static class KeyReader {
        public static Stream ReadStream(string name) {
            var assembly = typeof(KeyReader).Assembly;
            var resourceName = "Dalion.HttpMessageSigning.Keys." + name;

            return assembly.GetManifestResourceStream(resourceName);
        }

        public static RSAParameters ReadRSA(string name) {
            using (var stream = ReadStream(name)) {
                using (var reader = new PemReader(stream)) {
                    return reader.ReadRsaKey();
                }
            }
        }

        public static ECParameters ReadECDsaPrivate(string name) {
            using (var stream = ReadStream(name)) {
                using (var reader = new StreamReader(stream)) {
                    var fileContents = reader.ReadToEnd();
                    var lines = fileContents.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                    lines = lines.Skip(1).Take(lines.Length - 2).ToArray();
                    var pem = string.Join("", lines);
                    var ecdsa = ECDsa.Create();
                    var derArray = Convert.FromBase64String(pem);
                    ecdsa.ImportPkcs8PrivateKey(derArray, out _);
                    return ecdsa.ExportParameters(true);
                }
            }
        }
        
        public static ECParameters ReadECDsaPublic(string name) {
            using (var stream = ReadStream(name)) {
                using (var reader = new StreamReader(stream)) {
                    var fileContents = reader.ReadToEnd();
                    var lines = fileContents.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                    lines = lines.Skip(1).Take(lines.Length - 2).ToArray();
                    var pem = string.Join("", lines);
                    var ecdsa = ECDsa.Create();
                    var derArray = Convert.FromBase64String(pem);
                    ecdsa.ImportSubjectPublicKeyInfo(derArray, out _);
                    return ecdsa.ExportParameters(false);
                }
            }
        }
    }
}