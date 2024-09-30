using System;
using System.Text;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning {
    internal static class NonceAppenderExtensions {
        public static string BuildStringToAppend(this INonceAppender nonceAppender, string nonce) {
            if (nonceAppender == null) throw new ArgumentNullException(nameof(nonceAppender));
            
            var sb = new StringBuilder();
            nonceAppender.Append(nonce, sb);
            return sb.ToString();
        }
    }
}