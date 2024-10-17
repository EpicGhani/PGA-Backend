namespace Item.API.Models
{
    public class ItemDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string ScalingCollectionName { get; set; } = null!;
    }
}
