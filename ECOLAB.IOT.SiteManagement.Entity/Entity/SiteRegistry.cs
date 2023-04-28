namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    public class SiteRegistry
    {
        public string Id { get; set; }

        public string SiteId { get; set; }

        public string Model { get; set; }

        public string SourceUrl { get; set; }

        public string TargetUrl { get; set; }

        public string Checksum { get; set; }

        public string JObject { get; set; }

        public string Version { get; set; }

        public List<SiteDevice>? SiteDevices { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
