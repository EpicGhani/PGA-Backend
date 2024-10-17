using NFT.API.Models.Clubs.Utils;
using NFT.API.Models.Clubs.Interface;

namespace NFT.API.Models.Clubs
{
    public class ClubDataModel : IRarity
    {
        public string brandId { get; set; } = null!;
        public HeadData headData { get; set; } = null!;
        public ShaftData shaftData { get; set; } = null!;
        public GripData gripData { get; set; } = null!;

        public ClubDataModel(HeadData headData, ShaftData shaftData, GripData gripData, string brandId)
        { 
            this.headData = headData;
            this.shaftData = shaftData;
            this.gripData = gripData;
            this.brandId = brandId;
        }

        public Rarity GetRarity()
        {
            var value = headData.power + headData.control + headData.pitch + shaftData.accuracy + shaftData.durability + shaftData.tempo;
            return RarityUtility.ToRarity(value, 600);
        }
    }
}
