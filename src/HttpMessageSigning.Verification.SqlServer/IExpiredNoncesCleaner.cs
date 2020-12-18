using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface IExpiredNoncesCleaner : IDisposable {
        Task CleanUpNonces();
    }
}