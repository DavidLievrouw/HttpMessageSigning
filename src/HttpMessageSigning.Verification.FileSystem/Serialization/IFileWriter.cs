using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface IFileWriter {
        Task Write(string filePath, XDocument xml);
    }
}