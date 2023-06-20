namespace ECOLAB.IOT.SiteManagement.Common.Utilities
{
    using System;

    public static class StringExtensions
    {
        public static string AddPrefix(this string s, string prefix)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            string str = prefix + s;
            return str;
        }
    }
}
