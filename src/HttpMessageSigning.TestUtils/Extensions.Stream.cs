using System.IO;

namespace Dalion.HttpMessageSigning.TestUtils {
    public static partial class Extensions {
        public static byte[] ReadToByteArray(this Stream input) {
            if (input == null) return null;

            if (input is MemoryStream memStream) {
                return memStream.ToArray();
            }

            if (input.CanSeek) {
                input.Seek(0, SeekOrigin.Begin);
            }

            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream()) {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}