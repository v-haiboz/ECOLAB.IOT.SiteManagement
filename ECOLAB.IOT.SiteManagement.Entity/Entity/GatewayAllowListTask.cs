namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    public class GatewayAllowListTask
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string SiteNo { get; set; }
        public int GatewayId { get; set; }
        public string GatewayNo { get; set; }
        public string AllowListUrl { get; set; }
        public int Status { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
