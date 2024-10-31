using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Account.API.Models.Profile;
using Account.API.Models.Profile.Utils;
using MongoDB.Bson.IO;
using System.Text.Json;

namespace Account.API.Services
{
    public class AccountService
    {
        private readonly IMongoCollection<ProfileModel> _profiles;
        private string[] prefixes = new string[0];
        private string[] adjectives = new string[0];
        private Random random;

        public AccountService(IOptions<ProfileDatabaseSettings> profileDatabaseSettings)
        {
            var mongoClient = new MongoClient(profileDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(profileDatabaseSettings.Value.DatabaseName);
            _profiles = mongoDatabase.GetCollection<ProfileModel>(profileDatabaseSettings.Value.ProfileCollectionName);

            // Initialize the random generator and load username data
            random = new Random();
            LoadUsernameData();
        }

        /// <summary>
        /// Loads the prefixes and adjectives from the JSON file for username generation.
        /// </summary>

        private void LoadUsernameData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "username_prefixes.json.json");
            System.Diagnostics.Debug.WriteLine("Looking for JSON file at path: " + filePath);

            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);

                // Deserialize using System.Text.Json
                var data = JsonSerializer.Deserialize<PrefixesAndAdjectivesData>(jsonData);

                if (data != null)
                {
                    prefixes = data.Prefixes;
                    adjectives = data.Adjectives;

                    // Log loaded data to verify correctness
                    Console.WriteLine("Loaded prefixes: " + string.Join(", ", prefixes));
                    Console.WriteLine("Loaded adjectives: " + string.Join(", ", adjectives));
                }
            
                else
                {
                    throw new InvalidOperationException("Failed to deserialize username data.");
                }
            }
            else
            {
                throw new FileNotFoundException("username_prefixes.json file not found.");
            }
        }

        #region Profile Services

        public async Task<ProfileModel?> GetProfileAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var filter = Builders<ProfileModel>.Filter.Eq(i => i.Profile.UserId, userId);
            return await _profiles.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ProfileModel?> GetProfileByIDAsync(string id) =>
            await _profiles.Find(i => i.id == id).FirstOrDefaultAsync();

        #endregion

        #region Create and Update Profile

        /// <summary>
        /// Retrieves an existing profile or creates a new one with default data.
        /// </summary>
        public async Task<ProfileModel> GetOrCreateProfileAsync(string userId)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(i => i.Profile.UserId, userId);
            var profile = await _profiles.Find(filter).FirstOrDefaultAsync();

            if (profile == null)
            {
                // Generate a unique username
                string uniqueUsername = await GenerateUniqueUsername();

                // Define default data for the new profile
                profile = new ProfileModel
                {
                    Profile = new ProfileData
                    {
                        UserId = userId,
                        Username = uniqueUsername,
                        Password = string.Empty,
                        Email = string.Empty,
                        ProfilePicture = string.Empty,
                        BannerPicture = string.Empty,
                        Club = string.Empty,
                        Currency = 400,
                        PremiumCurrency = 30,
                        GooglePlayId = string.Empty,
                        AppleGameCenterId = string.Empty,
                        FacebookId = string.Empty
                    },
                    ExternalReference = new ExternalReference
                    {
                        Inventory = string.Empty,
                        Progression = string.Empty,
                        History = string.Empty,
                        Transaction = string.Empty
                    }
                };

                await _profiles.InsertOneAsync(profile);
            }
            return profile;
        }

        public async Task UpdateProfileAsync(string id, ProfileModel updatedProfileModel) =>
            await _profiles.ReplaceOneAsync(i => i.id == id, updatedProfileModel);

        public async Task UpdateProfileFieldAsync(string? id, UpdateDefinition<ProfileModel> updateDefinition)
        {
            await _profiles.UpdateOneAsync(
                Builders<ProfileModel>.Filter.Eq(p => p.id, id),
                updateDefinition
            );
        }

        #endregion

        #region Username Generation and Validation

        public async Task<string> GenerateUniqueUsername()
        {
            int maxRetries = 10000;

            for (int i = 0; i < maxRetries; i++)
            {
                string? prefix = prefixes[random.Next(prefixes.Length)];
                string? adjective = adjectives[random.Next(adjectives.Length)];
                string username = $"{prefix}{adjective}{random.Next(10, 100)}";

                if (!await IsUsernameInUse(username))
                {
                    return username;
                }
            }

            throw new Exception("Unable to generate a unique username after multiple attempts.");
        }

        public async Task<bool> IsUsernameInUse(string? username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.Username, username);
            var existingProfile = await _profiles.Find(filter).FirstOrDefaultAsync();
            return existingProfile != null;
        }

        /// <summary>
        /// Sets a unique username for the profile if it hasn't been set already.
        /// </summary>
        public async Task<bool> SetUsernameAsync(string userId, string username)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.UserId, userId);
            var profile = await _profiles.Find(filter).FirstOrDefaultAsync();

            if (profile == null)
                return false;

            if (!string.IsNullOrEmpty(profile.Profile.Username))
                throw new InvalidOperationException("Username has already been set and cannot be changed.");

            if (await IsUsernameInUse(username))
                throw new InvalidOperationException("Username is already in use.");

            var update = Builders<ProfileModel>.Update.Set(p => p.Profile.Username, username);
            var result = await _profiles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        #endregion

        #region Currency Management

        public async Task<bool> AddCurrencyAsync(string userId, int amount, bool isPremium = false)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be a positive integer.");

            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.UserId, userId);
            var update = isPremium
                ? Builders<ProfileModel>.Update.Inc(p => p.Profile.PremiumCurrency, amount)
                : Builders<ProfileModel>.Update.Inc(p => p.Profile.Currency, amount);

            var result = await _profiles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ConsumeCurrencyAsync(string userId, int amount, bool isPremium)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.UserId, userId) &
                         Builders<ProfileModel>.Filter.Gte(isPremium ? p => p.Profile.PremiumCurrency : p => p.Profile.Currency, amount);

            var update = isPremium
                ? Builders<ProfileModel>.Update.Inc(p => p.Profile.PremiumCurrency, -amount)
                : Builders<ProfileModel>.Update.Inc(p => p.Profile.Currency, -amount);

            var result = await _profiles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        #endregion
    }
}
