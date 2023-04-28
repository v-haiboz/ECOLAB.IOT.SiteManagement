namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using System;

    public class SiteMappingDevice
    {
        public float Id { get; set; }
        public string SiteNo { get; set; } = Guid.NewGuid().ToString();

        public DateTime SiteCreatedAt { get; set; } = DateTime.UtcNow;

        public int DeviceId { get; set; }

        public string DeviceNo { get; set; }

        public string JObject { get; set; }

        public DateTime DeviceCreatedAt { get; set; } = DateTime.UtcNow;

        public string Model { get; set; }

        public SiteMappingDeviceDto CovertToSiteMappingDeviceDto()
        {
            return new SiteMappingDeviceDto()
            {
                SiteId = SiteNo,
                SiteCreatedAt = SiteCreatedAt,
                DeviceId = DeviceNo,
                DeviceCreatedAt = DeviceCreatedAt,
                Model = Model
            };
        }
    }
}
