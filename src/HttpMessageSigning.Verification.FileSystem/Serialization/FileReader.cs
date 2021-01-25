using System;
using System.IO;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class FileReader : IFileReader {
        public string Read(string filePath) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));
            if (!FileExists(filePath)) return string.Empty;

            try {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using (var reader = new StreamReader(fileStream)) {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (FileNotFoundException) {
                return string.Empty;
            }
            catch (DirectoryNotFoundException) {
                return string.Empty;
            }
        }

        public bool FileExists(string filePath) {
            try {
                return File.Exists(filePath);
            }
            catch (FileNotFoundException) {
                return false;
            }
            catch (DirectoryNotFoundException) {
                return false;
            }
        }
    }
}