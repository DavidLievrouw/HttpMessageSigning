using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface INonceAppender {
        void Append(string nonce, StringBuilder sb);
    }
}