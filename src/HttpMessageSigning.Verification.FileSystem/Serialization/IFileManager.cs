using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface IFileManager<TData> {
        Task Write(IEnumerable<TData> data);
        Task<IEnumerable<TData>> Read();
    }
}