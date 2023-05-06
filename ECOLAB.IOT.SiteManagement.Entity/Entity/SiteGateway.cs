namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using System;

    public class SiteGateway
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public string GatewayNo { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
