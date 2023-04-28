namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GatewayHealthDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("site_id")]
        public string SiteNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("gateway")]
        public List<GatewayItemDto>? Gateways { get; set; }
    }

    public class GatewayItemDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public string GatewayNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("site_id")]
        public string SiteNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("connection_state")]
        public string ConnectionState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("last_seen")]
        public DateTime? Lastseen { get; set; }
    }
}
