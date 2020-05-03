using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal interface IDelayer {
        Task Delay(TimeSpan delay);
    }
}