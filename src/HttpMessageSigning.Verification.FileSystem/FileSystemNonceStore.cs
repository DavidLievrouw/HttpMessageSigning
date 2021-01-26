using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class FileSystemNonceStore : INonceStore {
        public void Dispose() {
            throw new System.NotImplementedException();
        }

        public Task Register(Nonce nonce) {
            throw new System.NotImplementedException();
        }

        public Task<Nonce> Get(KeyId clientId, string nonceValue) {
            throw new System.NotImplementedException();
        }
    }
}