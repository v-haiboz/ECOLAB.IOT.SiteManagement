namespace ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos
{
    using System;

    public class Permission
    {
        public string ModelName { get; set; } = "vcc";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
