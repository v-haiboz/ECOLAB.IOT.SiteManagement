namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using System;

    public class SiteDevice
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int SiteRegistryId { get; set; }

        public string DeviceNo { get; set;}

        public string JObjectInAllowList { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SiteDeviceDto CovertToSiteDeviceDto()
        {
            return new SiteDeviceDto()
            {
                SiteId = SiteId,
                SiteRegistryId = SiteRegistryId,
                DeviceNo = DeviceNo,
                JObjectInAllowList = JObjectInAllowList,
                UpdatedAt = UpdatedAt,
                CreatedAt = CreatedAt
            };
        }
    }
}
