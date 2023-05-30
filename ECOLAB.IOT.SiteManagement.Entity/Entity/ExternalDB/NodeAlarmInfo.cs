namespace ECOLAB.IOT.SiteManagement.Data.Entity.ExternalDB
{
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using System;

    public class NodeAlarmInfo
    {
        public string DeviceId { get; set; }
        public DateTime? Last_seen { get; set; }

        public string Mode { get; set; }

        public bool Low_alarm { get; set; }

        public string Status { get; set; }

        public long? Captured_at { get; set; }

        public DateTime? GetDateTime()
        {
            if (Captured_at != null && Captured_at.HasValue)
            {
                return Utility.ConvertTimeStampToDateTime(Captured_at.Value);
            }

            return null;
        }
    }
}
