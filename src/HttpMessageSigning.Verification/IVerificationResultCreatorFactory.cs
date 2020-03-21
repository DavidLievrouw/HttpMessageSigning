namespace Dalion.HttpMessageSigning.Verification {
    public interface IVerificationResultCreatorFactory {
        IVerificationResultCreator Create(Client client, Signature signature);
    }
}