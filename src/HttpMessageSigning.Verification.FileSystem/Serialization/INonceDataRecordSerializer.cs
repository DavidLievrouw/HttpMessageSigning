using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface INonceDataRecordSerializer {
        XContainer Serialize(NonceDataRecord nonce);
        NonceDataRecord Deserialize(XContainer data);
    }
}