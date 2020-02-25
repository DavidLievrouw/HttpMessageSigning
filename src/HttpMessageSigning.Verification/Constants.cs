namespace Dalion.HttpMessageSigning.Verification {
    public static class Constants {
        public static class ClaimTypes {
            public const string AppId = "appid";
            public const string Role = "role";
        }

        public static class AuthenticationSchemes {
            public const string HttpRequestSignature = "HttpRequestSignature";
        }
    }
}