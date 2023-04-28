namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using System;

    public class NodeFileInfo
    {
        public string DeviceId { get; set; }
        public DateTime? Last_seen { get; set; }

        public string Mode { get; set; }

        public string FileURL { get; set; }

        public string Status { get; set; }

        public DateTime? Captured_at
        {
            get
            {
                return GetDateTime();
            }
        }

        private DateTime? GetDateTime()
        {
            if (string.IsNullOrEmpty(FileURL))
            {
                return null;
            }

            var startIndex = FileURL.LastIndexOf('/');
            if (startIndex == -1)
            {
                return null;
            }

            var endIndex = FileURL.LastIndexOf(".jpg");
            if (endIndex == -1 || endIndex < startIndex)
            {
                return null;
            }

            var timespan = FileURL.Substring(startIndex + 1, endIndex - startIndex - 1);

            if (long.TryParse(timespan, out var timestamp))
            {
                return Utility.ConvertTimeStampToDateTime(timestamp);
            }

            return null;
        }
    }
}
