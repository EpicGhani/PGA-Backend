using NFT.API.Models.Clubs.Interface;

namespace NFT.API.Models.Clubs.Utils
{
    public class HeadData : IRarity
    {
        public string brandId { get; set; } = null!;
        public int power { get; set; }
        public int control { get; set; }
        public int pitch { get; set; }

        public HeadData(Rarity rarity)
        {
            power = rarity.RollValue();
            control = rarity.RollValue();
            pitch = rarity.RollValue();
        }

        public Rarity GetRarity()
        {
            var sum = power + control + pitch;
            return RarityUtility.ToRarity(sum, 300);
        }
    }
}
