namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using System;

    public class Site
    {
        public float Id { get; set; }
        public string SiteNo { get; set; }=Guid.NewGuid().ToString();

        public List<SiteRegistry> SiteRegistries { get; set; } = new List<SiteRegistry>();

        public DateTime UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
    }
}
