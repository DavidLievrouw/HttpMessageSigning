using System;

namespace Dalion.HttpMessageSigning.Verify {
    public class Tests {
        private readonly DateTimeOffset _now;

        public Tests() {
            _now = DateTimeOffset.Now;
        }
    }
}