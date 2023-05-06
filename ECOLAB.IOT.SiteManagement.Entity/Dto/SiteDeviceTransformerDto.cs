namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System.Collections.Generic;

    public class SiteDeviceTransformerDto
    {
        public string Version { get; set; } = "0.0.0";
        public List<DeviceTransformerDto> DeviceTransformerDtos { get; set; } = new List<DeviceTransformerDto>();
    }

    public class DeviceTransformerDto
    {
        public string DeviceNo { get; set; }

        public string JOjectInAllowList { get; set; }

        public string Json { get; set; }
    
    }
}
