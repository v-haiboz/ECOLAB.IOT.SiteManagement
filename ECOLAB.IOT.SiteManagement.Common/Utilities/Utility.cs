namespace ECOLAB.IOT.SiteManagement.Common.Utilities
{
    using System;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;

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

        private const string hex_reg = @"[0-9a-fA-F]";
        private const string stringOrNumber = @"^[a-zA-Z0-9]*$";
        private const string stringOrNumberOrOthers = @"^[a-zA-Z0-9._//-]*$";
        private const string url = @"^(https?|ftp|file|ws)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";

        public static bool ValidateSN(string serial_num, out string message)
        {

            var index = serial_num.IndexOf("-");
            if (index > -1)
            {
                serial_num = serial_num.Substring(index+1);
            }

            //return Regex.IsMatch(serial_num, @"^(DMC|CON|INT|CWT)[1-9]\d{8}$");
            if (string.IsNullOrEmpty(serial_num) || serial_num.Length != 12)
            {
                message = "serialNum length should be 12.";
                return false;
            }

            var temp = serial_num.Substring(0, 3);

            if (!IsAllEnglishChar(temp))
            {
                message = "{1-3} should be English letters";//;"{1-3} should be one of them(DMC|CON|INT|CWT).";
                return false;
            }
            serial_num = serial_num.Substring(3);
            temp = serial_num.Substring(0, 2);
            if (!int.TryParse(temp, out _))
            {
                message = "{4-5} should be a number";
                return false;
            }
            serial_num = serial_num.Substring(2);
            temp = serial_num.Substring(0, 2);
            if (!int.TryParse(temp, out int month) || month > 12)
            {
                message = "{6-7} should be a number,it should be less than or equal to 12";
                return false;
            }
            serial_num = serial_num.Substring(2);
            temp = serial_num.Substring(0, 2);
            if (!int.TryParse(temp, out int day))
            {
                message = "{8-9} should be a number";
                return false;
            }

            if (month == 02 && day > 29)
            {
                message = "If {6-7} is 02,{8-9} it should be less than or equal to 29.";
                return false;
            }

            if ((month == 04
               || month == 06
               || month == 09
               || month == 11) && day > 30)
            {
                message = "If {6-7} it is a leap month,{8-9} should be less than or equal to 31.";
                return false;
            }

            if ((month == 01
                || month == 03
                || month == 05
                || month == 07
                || month == 08
                || month == 10
                || month == 12) && day > 31)
            {
                message = "If {6-7} it is a non-leap month,{8-9} should be less than or equal to 30.";
                return false;
            }

            serial_num = serial_num.Substring(2);
            if (!int.TryParse(serial_num, out _))
            {
                message = "{10-12} should be a number";
                return false;
            }
            message = "";
            return true;
        }
        public static bool IsAllEnglishChar(string strValue)
        {
            bool result = Regex.IsMatch(strValue, @"^[A-Za-z]+$");
            return result;
        }

        //private static Dictionary<string, string> prefixs_dgw = new Dictionary<string, string>() {
        //    { "DGW","DGW"},{"MGW","PEC" }
        //};

        //public static string GenaratePrefix(string serial_num)
        //{
        //    var index = serial_num.IndexOf("-");
        //    if (index > -1)
        //    {
        //        serial_num = serial_num.Substring(index + 1);
        //    }

        //    if (string.IsNullOrEmpty(serial_num) || serial_num.Length != 12)
        //    {
        //        throw new Exception("SN length should be 12.");
        //    }

        //    var prefix = serial_num.Substring(0, 3);

        //    if (prefixs_dgw.TryGetValue(prefix, out var value))
        //    {
        //        return value;
        //    }

        //    throw new Exception("Currently only supports DGW and MGW Prefix.");
        //}
    }
}
