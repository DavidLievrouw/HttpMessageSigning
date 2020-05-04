using System.IO;
using System.Reflection;

namespace Dalion.HttpMessageSigning.SystemTests.Conformance.HttpMessages {
    public static class HttpMessageReader {
        public static string Read(string name) {
            var assembly = typeof(HttpMessageReader).Assembly;
            var resourceName = "Dalion.HttpMessageSigning.SystemTests.Conformance.HttpMessages." + name + ".httpMessage";

            using (var stream = assembly.GetManifestResourceStream(resourceName)) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}