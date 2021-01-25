﻿using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        /// <summary>
        ///     Load the public key parameters in the specified <see cref="RSA" />, defined by the specified XML string.
        /// </summary>
        /// <param name="rsa">The <see cref="RSA" /> to load the parameters in.</param>
        /// <param name="xmlString">The XML string that describes the public key parameters.</param>
        public static void FromXml(this RSA rsa, string xmlString) {
            if (string.IsNullOrEmpty(xmlString)) throw new ArgumentException("Value cannot be null or empty.", nameof(xmlString));

            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            // ReSharper disable once PossibleNullReferenceException
            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue")) {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) {
                    switch (node.Name) {
                        case "Modulus":
                            parameters.Modulus = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Exponent":
                            parameters.Exponent = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "P":
                            parameters.P = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Q":
                            parameters.Q = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "DP":
                            parameters.DP = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "DQ":
                            parameters.DQ = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "InverseQ":
                            parameters.InverseQ = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                        case "D":
                            parameters.D = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText);
                            break;
                    }
                }
            }
            else {
                throw new SerializationException($"Could not read {nameof(RSA)} parameters from the specified XML string.");
            }

            rsa.ImportParameters(parameters);
        }
    }
}