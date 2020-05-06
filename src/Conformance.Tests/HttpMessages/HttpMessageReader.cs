using System.IO;

namespace Dalion.HttpMessageSigning.HttpMessages {
    public static class HttpMessageReader {
        public static string Read(string name) {
            var assembly = typeof(HttpMessageReader).Assembly;
            var resourceName = "Dalion.HttpMessageSigning.HttpMessages." + name + ".httpMessage";

            using (var stream = assembly.GetManifestResourceStream(resourceName)) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}