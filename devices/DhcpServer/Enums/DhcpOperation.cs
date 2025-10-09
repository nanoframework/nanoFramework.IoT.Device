// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DhcpServer.Enums
{
    /// <summary>
    /// DHCP Operation.
    /// </summary>
    public enum DhcpOperation : byte
    {
        /// <summary>Boot request.</summary>
        BootRequest = 0x01,

        /// <summary>Boot reply.</summary>
        BootReply
    }

#if DEBUG

    /// <summary>
    /// Extensions for the <see cref="DhcpOperation"/> enumeration.
    /// </summary>
    public static class DhcpOperationExtensions
    {
        /// <summary>
        /// Converts the DHCP operation to a string.
        /// </summary>
        /// <param name="operation">The DHCP operation.</param>
        /// <returns>The string representation of the DHCP operation.</returns>
        public static string AsString(this DhcpOperation operation)
        {
            switch (operation)
            {
                case DhcpOperation.BootRequest:
                    return "BootRequest";

                case DhcpOperation.BootReply:
                    return "BootReply";

                default:
                    return "Unknown (" + ((byte)operation).ToString() + ")";
            }
        }
    }

#endif
}
