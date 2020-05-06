using System.IO;

namespace Dalion.HttpMessageSigning.Keys {
    public static class KeyReader {
        public static Stream Read(string name) {
            var assembly = typeof(KeyReader).Assembly;
            var resourceName = "Dalion.HttpMessageSigning.Keys." + name;

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}