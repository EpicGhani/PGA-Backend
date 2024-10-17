using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Item.API.Models;
using Item.API.Models.Clubs;

namespace Item.API.Services
{
    public class ItemService
    {
        private readonly IMongoCollection<Scaling> _scalingBank;

        public ItemService(IOptions<ItemDatabaseSettings> settings)
        {
            // DATABASE CONNECTION
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

            // COLLECTION REFERENCE
            _scalingBank = mongoDatabase.GetCollection<Scaling>(settings.Value.ScalingCollectionName);
        }

        #region Club Related Services
        // GET CLUB SCALING FOR DATA TABLE
        public async Task<List<Scaling>> GetScalingsAsync() =>
            await _scalingBank.Find(i => true).ToListAsync();

        // GET SCALING BY TAG
        public async Task<Scaling?> GetScalingByTagAsync(string tag)
        {
            var filter = Builders<Scaling>.Filter.Eq(i => i.scaleTag, tag.Trim().ToLower());

            return await _scalingBank.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<HeadData> GenerateHeadData()
        {
            return null;
        }
        public async Task<ShaftData> GenerateShaftData()
        {
            return null;
        }
        public async Task<GripData> GenerateGripData()
        {
            return null;
        }
        #endregion
    }
}