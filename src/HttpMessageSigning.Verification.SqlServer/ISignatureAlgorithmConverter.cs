namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface ISignatureAlgorithmConverter {
        void SetSignatureAlgorithm(ClientDataRecord dataRecord, ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey);
        ISignatureAlgorithm ToSignatureAlgorithm(ClientDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey);
    }
}