using System;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class ClientDataRecordSerializer : IClientDataRecordSerializer {
        public XContainer Serialize(ClientDataRecord client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            return client.ToXml();
        }

        public ClientDataRecord Deserialize(XContainer data) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            return ClientDataRecord.FromXml(data);
        }
    }
}