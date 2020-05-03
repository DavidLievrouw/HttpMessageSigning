using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class Delayer : IDelayer {
        public Task Delay(TimeSpan delay) {
            return Task.Delay(delay);
        }
    }
}