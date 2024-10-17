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

        public async Task<List<Scaling>> GetScalingsAsync() =>
            await _scalingBank.Find(i => true).ToListAsync();
    }
}