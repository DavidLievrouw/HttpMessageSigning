namespace Dalion.HttpMessageSigning.Verification {
    public class HttpRequestForVerification : HttpRequestForSigning {
        public byte[] Body { get; set; }
        public Signature Signature { get; set; }
        
        public override object Clone() {
            return new HttpRequestForVerification {
                Method = Method,
                RequestUri = RequestUri,
                Body = (byte[]) Body?.Clone(),
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary()),
                Signature = (Signature)Signature?.Clone()
            };
        }
    }
}