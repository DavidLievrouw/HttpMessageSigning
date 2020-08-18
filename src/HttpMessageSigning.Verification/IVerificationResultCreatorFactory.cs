namespace Dalion.HttpMessageSigning.Verification {
    internal interface IVerificationResultCreatorFactory {
        IVerificationResultCreator Create(Client client, Signature signature);
    }
}