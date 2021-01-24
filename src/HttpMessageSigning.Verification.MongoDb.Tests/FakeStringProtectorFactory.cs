using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class FakeStringProtectorFactory : IStringProtectorFactory {
        public IStringProtector CreateSymmetric(string secret) {
            return new FakeStringProtector();
        }
    }
}