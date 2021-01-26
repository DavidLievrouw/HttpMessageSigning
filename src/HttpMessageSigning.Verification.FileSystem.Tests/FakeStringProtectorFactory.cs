using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class FakeStringProtectorFactory : IStringProtectorFactory {
        public IStringProtector CreateSymmetric(string secret) {
            return new FakeStringProtector();
        }
    }
}