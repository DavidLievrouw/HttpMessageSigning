using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Utils {
    internal class Delayer : IDelayer {
        public Task Delay(TimeSpan delay) {
            return Task.Delay(delay);
        }
    }
}