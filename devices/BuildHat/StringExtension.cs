

using System;
using System.Collections;

namespace Iot.Device.BuildHat
{
    internal static class StringExtension
    {
        public static bool SequenceEqual(this string[] tocompare, string[] seq)
        {
            if ((tocompare == null) || (seq == null))
            {
                return false;
            }

            if (tocompare.Length != seq.Length)
            {
                return false;
            }

            for (int i = 0; i < tocompare.Length; i++)
            {
                if (seq[i] != tocompare[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string[] CleanEmpltySpaces(this string[] origin)
        {
            ArrayList toreturn = new();
            for (int i = 0; i < origin.Length; i++)
            {
                if (!string.IsNullOrEmpty(origin[i]))
                {
                    toreturn.Add(origin[i]);
                }
            }

            return (string[])toreturn.ToArray();
        }

        /// <summary>
        /// Converts an ISO 8601 time/date format string, which is used by JSON and others,
        /// into a DateTime object.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string date)
        {

            // Check to see if format contains the timezone ID, or contains UTC reference
            // Neither means it's local time
            bool utc = date.EndsWith("Z");

            string[] parts = date.Split(new char[] { 'T', 'Z', ':', '-', '.', '+', });

            // We now have the time string to parse, and we'll adjust
            // to UTC or timezone after parsing
            string year = parts[0];
            string month = (parts.Length > 1) ? parts[1] : "1";
            string day = (parts.Length > 2) ? parts[2] : "1";
            string hour = (parts.Length > 3) ? parts[3] : "0";
            string minute = (parts.Length > 4) ? parts[4] : "0";
            string second = (parts.Length > 5) ? parts[5] : "0";
            string ms = (parts.Length > 6) ? parts[6] : "0";

            // Check if any of the date time parts is non-numeric
            if (!IsNumeric(year))
            {
                return DateTime.MaxValue;
            }
            else if (!IsNumeric(month))
            {
                return DateTime.MaxValue;
            }
            else if (!IsNumeric(day))
            {
                return DateTime.MaxValue;
            }
            else if (!IsNumeric(hour))
            {
                return DateTime.MaxValue;
            }
            else if (!IsNumeric(minute))
            {
                return DateTime.MaxValue;
            }
            else if (!IsNumeric(second))
            {
                return DateTime.MaxValue;
            }
            else if (!IsNumeric(ms))
            {
                return DateTime.MaxValue;
            }

            // sanity check for bad milliseconds format
            int milliseconds = Convert.ToInt32(ms);

            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            DateTime dt = new(
                Convert.ToInt32(year),
                Convert.ToInt32(month),
                Convert.ToInt32(day),
                Convert.ToInt32(hour),
                Convert.ToInt32(minute),
                Convert.ToInt32(second),
                milliseconds);

            if (utc)
            {
                // Convert the Kind to DateTimeKind.Utc if string Z present
                dt = new DateTime(dt.Ticks, DateTimeKind.Utc); //TODO!!!
            }
            else
            {
                //nF does not support non UTC dates, so should we throw an exception instead.
                throw new NotSupportedException();
            }

            return dt;
        }

        /// <summary>
        /// Returns true if the given string contains only numeric characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool IsNumeric(string str)
        {
            foreach (char c in str)
            {
                if (!((c >= '0') && (c <= '9')))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
