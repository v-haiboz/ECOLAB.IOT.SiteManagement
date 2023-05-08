namespace ECOLAB.IOT.SiteManagement.Data.Exceptions
{
    /// <summary>
    /// Costom Exception
    /// </summary>
    public class BizException : Exception
    {
        public int status = 601;
        /// <summary>
        /// 
        /// </summary>
        public BizException(int status = 601)
        {
            this.status = status;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public BizException(string message, int status = 601) : base(message)
        {
            this.status = status;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public BizException(string message, Exception ex, int status = 601) : base(message, ex)
        {
            this.status = status;
        }
    }
}
