namespace Dalion.HttpMessageSigning.Utils {
    internal class StringProtectorFactory : IStringProtectorFactory {
        public IStringProtector CreateSymmetric(string secret) {
            return new SymmetricStringProtector(secret);
        }
    }
}