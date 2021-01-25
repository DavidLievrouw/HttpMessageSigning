namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface IFileWriter {
        void Write(string filePath, string textToBeWritten);
    }
}