using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Bitmask specifying which operations can be done.
    /// </summary>
    [Flags]
    public enum AccessPermissions : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Read bit.
        /// </summary>
        Read = 0x01,

        /// <summary>
        /// Write bit.
        /// </summary>
        Write = 0x02,

        /// <summary>
        /// Write without response bit.
        /// </summary>
        WriteWithoutResponse = 0x04,

        /// <summary>
        /// Signed write bit.
        /// </summary>
        SignedWrite = 0x08
    }
}