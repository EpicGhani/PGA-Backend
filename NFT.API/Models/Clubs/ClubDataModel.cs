using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NFT.API.Models.Clubs.Utils;
using NFT.API.Models.Clubs.Interface;

namespace NFT.API.Models.Clubs
{
    public class ClubDataModel : IRarity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; } = null!;
        public string brandId { get; set; } = null!;
        public HeadData headData { get; set; } = null!;
        public ShaftData shaftData { get; set; } = null!;
        public GripData gripData { get; set; } = null!;

        public ClubDataModel(HeadData headData, ShaftData shaftData, GripData gripData)
        { 
            this.headData = headData;
            this.shaftData = shaftData;
            this.gripData = gripData;
        }

        public Rarity GetRarity()
        {
            var value = headData.power + headData.control + headData.pitch + shaftData.accuracy + shaftData.durability + shaftData.tempo;
            return RarityUtility.ToRarity(value, 600);
        }
    }
}
