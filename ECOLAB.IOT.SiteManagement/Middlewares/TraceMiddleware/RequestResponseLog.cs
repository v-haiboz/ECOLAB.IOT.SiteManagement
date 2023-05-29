namespace ECOLAB.IOT.SiteManagement.Middlewares.TraceMiddleware
{
    public class RequestResponseLog
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Token Info
        /// </summary>
        public string TokenInfo { get; set; }
        /// <summary>
        /// APICertificate
        /// </summary>
        public string APICertificate { get; set; }
        /// <summary>
        /// Method
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// Query String
        /// </summary>
        public string QueryString { get; set; }
        /// <summary>
        /// Request Body
        /// </summary>
        public string RequestBody { get; set; }
        /// <summary>
        /// Response Body
        /// </summary>
        public string ResponseBody { get; set; }
        /// <summary>
        /// Excute Start Time
        /// </summary>
        public DateTime ExcuteStartTime { get; set; } = DateTime.Now;
        /// <summary>
        /// Excute End Time
        /// </summary>
        public DateTime ExcuteEndTime { get; set; }
        /// <summary>
        /// To Log String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //string headers = "[" + string.Join(",", this.Headers.Select(i => "{" + $"\"{i.Key}\":\"{i.Value}\"" + "}")) + "]";
            return $"Url: {Url},\r\nTokenInfo: {TokenInfo},\r\nAPICertificate: {APICertificate},\r\nMethod: {Method},\r\nQueryString: {QueryString},\r\nRequestBody: {RequestBody},\r\nResponseBody: {ResponseBody},\r\nExcuteStartTime: {ExcuteStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")},\r\nExcuteStartTime: {ExcuteEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
        }
    }
}
