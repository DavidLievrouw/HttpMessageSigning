using System;
using System.Text;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning {
    internal static class HeaderAppenderExtensions {
        public static string BuildStringToAppend(this IHeaderAppender headerAppender, HeaderName header) {
            if (headerAppender == null) throw new ArgumentNullException(nameof(headerAppender));

            var sb = new StringBuilder();
            headerAppender.Append(header, sb);
            return sb.ToString();
        }
    }
}