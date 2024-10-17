using NFT.API.Models.Clubs;
using NFT.API.Models.Clubs.Interface;
using NFT.API.Models.Clubs.Utils;

namespace NFT.API.Services
{
    public class NFTService
    {
        private Rarity rarity;

        #region Craft Club
        public async Task<ClubDataModel> CraftClub(int rarityIndex)
        {
            rarity = (Rarity)rarityIndex;

            var headData = new HeadData(rarity);
            var shaftData = new ShaftData(rarity);
            var gripData = new GripData();
            var clubData = new ClubDataModel(headData, shaftData, gripData);

            return clubData;
        }
        #region Generate Club Parts
        public async Task<HeadData> GenerateHeadData()
        {
            // Generate head data and then save to the item bank
            return null;
        }

        public async Task<ShaftData> GenerateShaftData()
        {
            // Generate shaft data and then save to the item bank
            return null;
        }

        public async Task<GripData> GenerateGripData()
        {
            // Generate grip data and then save to the item bank
            return null;
        }
        #endregion
        #endregion

    }
}
