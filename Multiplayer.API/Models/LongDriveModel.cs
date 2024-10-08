﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Multiplayer.API.Models.Utility;

namespace Multiplayer.API.Models
{
    public class LongDriveModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string userId { get; set; } = null!;
        public playerTurnsData[] playerTurnsData { get; set; } = null!;
    }
}
