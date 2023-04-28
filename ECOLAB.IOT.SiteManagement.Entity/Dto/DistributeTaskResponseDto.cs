namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    public class DistributeTaskDataDto
    {
        /// <summary>
        /// 
        /// </summary>
        public float dateTimeUTC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float enqueuedTimeUtc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestampIoTHubEnqueuedTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestampEventRouteReceive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestampEventHubEnqueuedTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestampDeviceAPIReceive { get; set; }
    }

    public class DistributeTaskResponseDto
    {
        /// <summary>
        /// 
        /// </summary>
        public DistributeTaskDataDto data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> errors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sendStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string executeStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string detailInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestamp { get; set; }
    }
}
