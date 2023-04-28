namespace ECOLAB.IOT.SiteManagement.Common.Utilities
{
    using System;

    public class Utility
    {

        public static DateTime ConvertTimeStampToDateTime(long timeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); 
            var time = startTime.AddSeconds(timeStamp);

            return time;

        }
    }
}
