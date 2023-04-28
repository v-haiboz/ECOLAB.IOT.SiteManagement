namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class SiteMappingDeviceDto
    {
        public string SiteId { get; set; }

        public DateTime SiteCreatedAt { get; set; }

        public string DeviceId { get; set; }

        public DateTime DeviceCreatedAt { get; set; }

        public string Model { get; set; }
    }

}
