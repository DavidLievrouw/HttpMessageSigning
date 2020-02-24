namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppender {
        string BuildStringToAppend(HeaderName header);
    }
}