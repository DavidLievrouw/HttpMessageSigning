namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface IFileReader {
        string Read(string filePath);
        bool FileExists(string filePath);
    }
}