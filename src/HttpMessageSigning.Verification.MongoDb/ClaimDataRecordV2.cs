﻿using System;
using System.Security.Claims;
using MongoDB.Bson.Serialization.Attributes;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [BsonIgnoreExtraElements]
    internal class ClaimDataRecordV2 {
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        
        public Claim ToClaim() {
            return new Claim(Type, Value, ValueType, Issuer, OriginalIssuer);
        }

        public static ClaimDataRecordV2 FromClaim(Claim claim) {
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            
            return new ClaimDataRecordV2 {
                Issuer = claim.Issuer,
                Type = claim.Type,
                Value = claim.Value,
                OriginalIssuer = claim.OriginalIssuer,
                ValueType = claim.ValueType
            };
        }
    }
}