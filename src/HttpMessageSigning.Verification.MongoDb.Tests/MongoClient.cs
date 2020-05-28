﻿using MongoDB.Driver;

 namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoClient {
        public static IMongoClient Create(string connectionString) {
            var mongoUrl = new MongoUrl(connectionString);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);
            return new MongoDB.Driver.MongoClient(mongoClientSettings);
        }
    }
}