namespace Dalion.HttpMessageSigning.Composing {
    internal interface IHeaderAppender {
        string BuildStringToAppend(HeaderName header);
    }
}