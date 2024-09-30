using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class NonceAppender : INonceAppender {
        private const string Header = "\nnonce: ";

        public void Append(string nonce, StringBuilder sb) {
            if (string.IsNullOrEmpty(nonce)) return;
            sb.Append(Header);
            sb.Append(nonce);
        }
    }
}