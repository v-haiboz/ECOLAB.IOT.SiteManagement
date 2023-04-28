namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    public class GatewayAllowListTaskHistory
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string SiteNo { get; set; }
        public int GatewayId { get; set; }
        public string GatewayNo { get; set; }
        public string AllowListSASUrl { get; set; }
        public int Status { get; set; } = 1;

        public DateTime UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
    }
}
