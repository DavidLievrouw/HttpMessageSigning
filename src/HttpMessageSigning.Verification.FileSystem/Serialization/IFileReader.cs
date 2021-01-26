using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface IFileReader {
        Task<XDocument> Read(string filePath);
        bool FileExists(string filePath);
    }
}