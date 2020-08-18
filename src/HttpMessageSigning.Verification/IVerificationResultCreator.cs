namespace Dalion.HttpMessageSigning.Verification {
    internal interface IVerificationResultCreator {
        RequestSignatureVerificationResult CreateForSuccess();
        RequestSignatureVerificationResult CreateForFailure(SignatureVerificationFailure failure);
    }
}