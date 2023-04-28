namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class DeviceHealthDto
    {
        public string? Id { get; set; }

        public string? SiteId { get; set; }

        public string? Connection_state { get; set; } = "offline";

        public DateTime? Last_seen { get; set; }

        public LastEvent? Last_event { get; set; }
    }

    public class LastEvent
    { 
        public string? Image { get; set; }

        public DateTime? Captured_at { get; set; }
    }
}
