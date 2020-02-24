namespace Dalion.HttpMessageSigning {
    internal interface IBase64Converter {
        byte[] FromBase64(string base64);
        string ToBase64(byte[] bytes);
    }
}