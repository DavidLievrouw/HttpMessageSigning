using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        internal static void FromXml(this ECDsa ecdsa, string xmlString) {
            if (string.IsNullOrEmpty(xmlString)) throw new ArgumentException("Value cannot be null or empty.", nameof(xmlString));
            
            var parameters = new ECParameters {
                Q = new ECPoint()
            };

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            // ReSharper disable once PossibleNullReferenceException
            if (xmlDoc.DocumentElement.Name.Equals("ECDsaKeyValue")) {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) {
                    switch (node.Name) {
                        case "FriendlyName":
                            parameters.Curve = ECCurve.CreateFromFriendlyName(node.InnerText);
                            break;
                        case "Q.X":
                            parameters.Q.X = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Q.Y":
                            parameters.Q.Y = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                    }
                }
            }
            else {
                throw new SerializationException($"Could not read {nameof(ECDsa)} parameters from the specified XML string.");
            }

            ecdsa.ImportParameters(parameters);
        }
    }
}