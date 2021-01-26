﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class FileWriter : IFileWriter {
        public async Task Write(string filePath, XDocument xml) {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
#if NET472 || NETSTANDARD2_0
                xml.Save(fileStream, SaveOptions.DisableFormatting);
#else
                await xml.SaveAsync(fileStream, SaveOptions.DisableFormatting, CancellationToken.None);
#endif
                await fileStream.FlushAsync(CancellationToken.None);
            }
        }
    }
}