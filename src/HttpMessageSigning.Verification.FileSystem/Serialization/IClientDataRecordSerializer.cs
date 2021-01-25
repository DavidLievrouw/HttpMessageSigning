using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal interface IClientDataRecordSerializer {
        XContainer Serialize(ClientDataRecord client);
        ClientDataRecord Deserialize(XContainer data);
    }
}