using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppender {
        void Append(HeaderName header, StringBuilder sb);
    }
}