namespace Dalion.HttpMessageSigning.Signing {
    internal interface IAuthorizationHeaderParamCreator {
        string CreateParam(Signature signature);
    }
}