
namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SiteDeviceDetailInfoDto
    {
        public int SiteId { get; set; }

        public string SiteNo { get; set; }

        public int SiteRegistryId { get; set; }

        public string Model { get; set; }

        public int GatewayId { get; set; }

        public string GatewayNo { get; set; }

        public string DeviceNo { get; set; }

        public string JObjectInAllowList { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
