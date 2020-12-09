using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface IDelayer {
        Task Delay(TimeSpan delay);
    }
}