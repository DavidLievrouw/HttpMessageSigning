namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface IStringProtector {
        string Protect(string plainText);
        string Unprotect(string cipherText);
    }
}