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
        public int FrameInterval { get; set; }
        public int TotalFrameCount { get; set; }
        public SwingData SwingData { get; set; } = null!;
        public StrikeData StrikeData { get; set; } = null!;
        public StateData[] States { get; set; } = null!;
    }
}
