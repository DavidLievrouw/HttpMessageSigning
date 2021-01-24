namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal interface ISignatureAlgorithmDataRecordConverter {
        SignatureAlgorithmDataRecordV2 FromSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey);
        ISignatureAlgorithm ToSignatureAlgorithm(SignatureAlgorithmDataRecordV2 dataRecord, SharedSecretEncryptionKey encryptionKey, int? recordVersion);
    }
}