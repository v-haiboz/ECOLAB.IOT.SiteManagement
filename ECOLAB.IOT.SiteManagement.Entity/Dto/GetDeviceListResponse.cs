namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class GetDeviceListResponse
    {
        [JsonPropertyName("site_id")]
        public string SiteNo { get; set; }

    }

    public class GetwayDeviceDto
    {
       [JsonPropertyName("id")]
       public string GatewayNo { get; set; }
    }
}
