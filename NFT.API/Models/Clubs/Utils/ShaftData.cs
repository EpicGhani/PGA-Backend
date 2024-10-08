using NFT.API.Models.Clubs.Interface;

namespace NFT.API.Models.Clubs.Utils
{
    public class ShaftData : IRarity
    {
        public string brandId { get; set; } = null!;
        public int accuracy { get; set; }
        public int tempo { get; set; }
        public int durability { get; set; }

        public ShaftData(Rarity rarity)
        {
            accuracy = rarity.RollValue();
            durability = rarity.RollValue();
            tempo = rarity.RollValue();
        }

        public Rarity GetRarity()
        {
            var sum = accuracy + durability + tempo;
            return RarityUtility.ToRarity(sum, 300);
        }
    }
}
