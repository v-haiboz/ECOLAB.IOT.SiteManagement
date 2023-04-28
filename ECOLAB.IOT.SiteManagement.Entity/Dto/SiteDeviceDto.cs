namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class SiteDeviceDto
    {
        public int SiteId { get; set; }

        public int SiteRegistryId { get; set; }

        public string DeviceNo { get; set; }

        public string JObjectInAllowList { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
