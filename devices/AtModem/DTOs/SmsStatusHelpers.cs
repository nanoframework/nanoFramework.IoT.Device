// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Helper for SMS status.
    /// </summary>
    public static class SmsStatusHelpers
    {
        /// <summary>
        /// Converts an integer status code to an SmsStatus enumeration value.
        /// </summary>
        /// <param name="statusCode">The status code to convert.</param>
        /// <returns>An SmsStatus value.</returns>
        public static SmsStatus ToSmsStatus(int statusCode)
        {
            switch (statusCode)
            {
                default:
                case 0:
                    return SmsStatus.ALL;
                case 1:
                    return SmsStatus.REC_READ;
                case 2:
                    return SmsStatus.REC_UNREAD;
                case 3:
                    return SmsStatus.STO_SENT;
                case 4:
                    return SmsStatus.STO_UNSENT;
            }
        }

        /// <summary>
        /// Converts a string representation of the status to an SmsStatus enumeration value.
        /// </summary>
        /// <param name="text">The string representation of the status.</param>
        /// <returns>An SmsStatus value.</returns>
        public static SmsStatus ToSmsStatus(string text)
        {
            switch (text)
            {
                default:
                case "ALL":
                    return SmsStatus.ALL;
                case "REC READ":
                    return SmsStatus.REC_READ;
                case "REC UNREAD":
                    return SmsStatus.REC_UNREAD;
                case "STO SENT":
                    return SmsStatus.STO_SENT;
                case "STO UNSENT":
                    return SmsStatus.STO_UNSENT;
            }
        }

        /// <summary>
        /// Converts an SmsStatus enumeration value to its string representation.
        /// </summary>
        /// <param name="smsStatus">The SmsStatus value to convert.</param>
        /// <returns>The string representation of the SmsStatus.</returns>
        public static string ToString(SmsStatus smsStatus)
        {
            switch (smsStatus)
            {
                default:
                case SmsStatus.ALL:
                    return "ALL";
                case SmsStatus.REC_READ:
                    return "REC READ";
                case SmsStatus.REC_UNREAD:
                    return "REC UNREAD";
                case SmsStatus.STO_SENT:
                    return "STO SENT";
                case SmsStatus.STO_UNSENT:
                    return "STO UNSENT";
            }
        }
    }
}
