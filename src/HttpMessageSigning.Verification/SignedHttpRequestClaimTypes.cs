namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Defines the known claim types of verified principals.
    /// </summary>
    public static class SignedHttpRequestClaimTypes {
        /// <summary>
        ///     The 'AppId' claim.
        /// </summary>
        public const string AppId = "appid";

        /// <summary>
        ///     The 'Name' claim.
        /// </summary>
        public const string Name = "name";

        /// <summary>
        ///     The 'Role' claim.
        /// </summary>
        public const string Role = "role";

        /// <summary>
        ///     The 'Version' claim.
        /// </summary>
        public const string Version = "ver";
    }
}