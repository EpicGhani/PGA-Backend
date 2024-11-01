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
                        ProfilePictureId = string.Empty,
                        BannerPictureId = string.Empty,
                        Club = string.Empty,
                        Currency = 400,
                        PremiumCurrency = 30,
                        RemainingUsernameChanges = 1,
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

        public async Task<UpdateResult> UpdateProfileFieldAsync(string? id, UpdateDefinition<ProfileModel> updateDefinition)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(p => p.id, id);
            return await _profiles.UpdateOneAsync(filter, updateDefinition);
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

            // Use a case-insensitive regex filter
            var filter = Builders<ProfileModel>.Filter.Regex(p => p.Profile.Username, new MongoDB.Bson.BsonRegularExpression($"^{username}$", "i"));
            var existingProfile = await _profiles.Find(filter).FirstOrDefaultAsync();
            return existingProfile != null;
        }


        /// <summary>
        /// Sets a unique username for the profile if it hasn't been set already.
        /// </summary>
        public async Task<SetUsernameResponse> SetUsernameAsync(string userId, string username)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.UserId, userId);
            var profile = await _profiles.Find(filter).FirstOrDefaultAsync();

            if (profile == null)
                throw new Exception("Profile not found.");

            if (profile.Profile.RemainingUsernameChanges <= 0)
                throw new InvalidOperationException("No remaining username changes allowed.");

            if (await IsUsernameInUse(username))
                throw new InvalidOperationException("Username is already in use.");

            // Update username and decrement RemainingUsernameChanges
            var update = Builders<ProfileModel>.Update
                .Set(p => p.Profile.Username, username)
                .Inc(p => p.Profile.RemainingUsernameChanges, -1);

            var result = await _profiles.UpdateOneAsync(filter, update);

            // If the update is successful, return the response model
            if (result.ModifiedCount > 0)
            {
                return new SetUsernameResponse
                {
                    Success = true,
                    RemainingUsernameChanges = profile.Profile.RemainingUsernameChanges - 1
                };
            }

            // If the update fails, return a failure response
            return new SetUsernameResponse
            {
                Success = false,
                RemainingUsernameChanges = profile.Profile.RemainingUsernameChanges
            };
        }

        public async Task<bool> UpdateProfilePictureAsync(string userId, int profilePictureId)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.UserId, userId);
            var update = Builders<ProfileModel>.Update.Set("Profile.ProfilePictureId", profilePictureId);
            var result = await _profiles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateBannerPictureAsync(string userId, int bannerPictureId)
        {
            var filter = Builders<ProfileModel>.Filter.Eq(p => p.Profile.UserId, userId);
            var update = Builders<ProfileModel>.Update.Set("Profile.BannerPictureId", bannerPictureId);
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
