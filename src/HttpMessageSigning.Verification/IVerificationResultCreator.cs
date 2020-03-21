namespace Dalion.HttpMessageSigning.Verification {
    public interface IVerificationResultCreator {
        RequestSignatureVerificationResult CreateForSuccess();
        RequestSignatureVerificationResult CreateForFailure(SignatureVerificationFailure failure);
    }
}