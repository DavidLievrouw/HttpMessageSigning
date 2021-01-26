namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    /// <summary>
    ///     Represents settings for MongoDb storage of registered <see cref="Nonce" /> instances.
    /// </summary>
    public class FileSystemNonceStoreSettings {
        /// <summary>
        ///     Gets or sets the path to the data file. This path should contain the file name with extension.
        /// </summary>
        public string FilePath { get; set; }

        internal void Validate() {
            if (string.IsNullOrEmpty(FilePath)) throw new ValidationException($"The {nameof(FileSystemNonceStoreSettings)} do not specify a valid {nameof(FilePath)}.");
        }
    }
}