namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal interface IStringProtector {
        string Protect(string plainText);
        string Unprotect(string cipherText);
    }
}