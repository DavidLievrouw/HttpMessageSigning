using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Utils {
    internal interface IDelayer {
        Task Delay(TimeSpan delay);
    }
}