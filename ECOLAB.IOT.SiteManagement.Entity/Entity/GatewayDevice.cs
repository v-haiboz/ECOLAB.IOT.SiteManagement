namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using System;
    public class GatewayDevice
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int GatewayId { get; set; }

        public string DeviceNo { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
