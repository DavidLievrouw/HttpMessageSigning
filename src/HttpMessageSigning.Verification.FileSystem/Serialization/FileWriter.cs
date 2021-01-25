using System;
using System.IO;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class FileWriter : IFileWriter {
        public void Write(string filePath, string textToBeWritten) {
            if (textToBeWritten == null) throw new ArgumentNullException(nameof(textToBeWritten));
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                using (var streamWriter = new StreamWriter(fileStream)) {
                    streamWriter.Write(textToBeWritten);
                    streamWriter.Flush();
                    fileStream.Flush(flushToDisk: true);
                }
            }
        }
    }
}