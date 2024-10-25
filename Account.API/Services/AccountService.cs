using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Account.API.Models.Profile;

namespace Account.API.Services
{
    public class AccountService
    {
        private readonly IMongoCollection<ProfileModel> _profiles;

        public AccountService(IOptions<ProfileDatabaseSettings> profileDatabaseSettings) 
        {
            var mongoClient = new MongoClient(profileDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(profileDatabaseSettings.Value.DatabaseName);
            _profiles = mongoDatabase.GetCollection<ProfileModel>(profileDatabaseSettings.Value.ProfileCollectionName);
        }

        #region Get Profile Services
        public async Task<ProfileModel?> GetProfileAsync(string userId)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(i => i.Profile.UserId, userId);

            return await _profiles.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ProfileModel?> GetProfileByIDAsync(string id) =>
            await _profiles.Find(i => i.id == id).FirstOrDefaultAsync();
        #endregion

        #region Update & Create Profile Services
        public async Task UpdateProfileAsync(string id, ProfileModel updatedProfileModel) =>
            await _profiles.ReplaceOneAsync(i => i.id == id, updatedProfileModel);

        public async Task CreateProfileAsync(ProfileModel newProfileModel) =>
            await _profiles.InsertOneAsync(newProfileModel);
        #endregion 
    }
}
