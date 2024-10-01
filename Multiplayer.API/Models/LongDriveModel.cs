using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Multiplayer.API.Models.Utility;

namespace Multiplayer.API.Models
{
    public class LongDriveModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public int RngSeed { get; set; }
        public string[] TakeThatsToApplyToOpponent { get; set; } = null!;
        public RecordedStrike RecordedStrike { get; set; } = null!;
    }
}
