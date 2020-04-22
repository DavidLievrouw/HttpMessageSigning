namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static string ToSanitizedHttpHeaderName(this HeaderName headerName) {
            if (headerName.Equals(HeaderName.PredefinedHeaderNames.Created)) {
                return "Signature-Created";
            }           
            if (headerName.Equals(HeaderName.PredefinedHeaderNames.Expires)) {
                return "Signature-Expires";
            }
            return headerName.Value.Trim('(', ')');
        }
    }
}