namespace ECOLAB.IOT.SiteManagement.Common.Utilities
{
    using System;
    using System.Net.Sockets;

    public class Utility
    {
        public static DateTime ConvertTimeStampToDateTime(long timeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); 
            var time = startTime.AddSeconds(timeStamp);

            return time;

        }

        private static int _counter = 0;
        private static readonly object Locker = new object();
        public static string GenerateVersion()
        {

            return DateTime.UtcNow.Ticks.ToString();
            //lock (Locker)
            //{
            //    if (_counter == 1000)
            //    {
            //        _counter = 0;
            //    }
            //    else
            //    {
            //        _counter++;
            //    }
            //    Thread.Sleep(100);
            //    return DateTime.Now.ToString("yyyyMMddHHmmss") + _counter.ToString().PadLeft(3, '0');
            //}
        }
    }
}
