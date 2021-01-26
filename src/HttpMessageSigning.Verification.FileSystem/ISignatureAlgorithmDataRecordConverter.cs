namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal interface ISignatureAlgorithmDataRecordConverter {
        SignatureAlgorithmDataRecord FromSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey);
        ISignatureAlgorithm ToSignatureAlgorithm(SignatureAlgorithmDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey, int? recordVersion);
    }
}