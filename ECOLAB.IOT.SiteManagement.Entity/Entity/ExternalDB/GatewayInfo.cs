namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using System;
    using System.Text.Json.Serialization;

    public class GatewayInfo
    {

        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string ConnectionState { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
    }
}
