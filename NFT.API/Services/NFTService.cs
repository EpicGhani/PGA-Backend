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
        #endregion
    }


}
