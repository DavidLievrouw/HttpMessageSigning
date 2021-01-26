using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class FileReader : IFileReader {
        internal static readonly Lazy<XDocument> EmptyDocument = new Lazy<XDocument>(() => {
            var xmlRoot = new XElement(XName.Get(Constants.XmlNames.Root, Constants.XmlNames.Ns));
            xmlRoot.SetAttributeValue("xmlns", Constants.XmlNames.Ns);
            var xmlDocument = new XDocument(xmlRoot);
            return xmlDocument;
        });

        public async Task<XDocument> Read(string filePath) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));
            if (!FileExists(filePath)) return EmptyDocument.Value;

            try {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
#if NET472 || NETSTANDARD2_0
                    var doc = XDocument.Load(fileStream, LoadOptions.None);
                    return await Task.FromResult(doc);
#else
                    return await XDocument.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None);
#endif
                }
            }
            catch (FileNotFoundException) {
                return EmptyDocument.Value;
            }
            catch (DirectoryNotFoundException) {
                return EmptyDocument.Value;
            }
        }

        public bool FileExists(string filePath) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));
            
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