namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using System;

    public class SiteDeviceDetailInfo
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

        public SiteDeviceDetailInfoDto CovertToSiteDeviceDto()
        {
            return new SiteDeviceDetailInfoDto()
            {
                SiteId = SiteId,
                SiteNo=SiteNo,
                SiteRegistryId = SiteRegistryId,
                Model=Model,
                GatewayId=GatewayId,
                GatewayNo=GatewayNo,
                DeviceNo = DeviceNo,
                JObjectInAllowList = JObjectInAllowList,
                UpdatedAt = UpdatedAt,
                CreatedAt = CreatedAt
            };
        }
    }
}
