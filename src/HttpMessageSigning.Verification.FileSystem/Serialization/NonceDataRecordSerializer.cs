using System;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class NonceDataRecordSerializer : INonceDataRecordSerializer {
        public XContainer Serialize(NonceDataRecord nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));
            
            return nonce.ToXml();
        }

        public NonceDataRecord Deserialize(XContainer data) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            return NonceDataRecord.FromXml(data);
        }
    }
}