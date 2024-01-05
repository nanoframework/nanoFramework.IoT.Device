namespace Iot.Device.Max1704x
{
    internal enum Registers16 : ushort
    {
        /// <summary>
        /// On the MAX17048/49 this also clears the EnSleep bit.
        /// </summary>
        Max17043ModeQuickstart = 0x4000, 

        /// <summary>
        /// W - _Enables_ sleep mode (the SLEEP bit in the CONFIG reg engages sleep).
        /// </summary>
        Max17048ModeEnsleep = 0x2000,
        
        /// <summary>
        /// R - indicates when the IC is in hibernate mode.
        /// </summary>
        Max17048ModeHibstat = 0x1000,

        /// <summary>
        /// Specifies the Max1704XStatusEnVr member of the Register16 enumeration. This member indicates the Enable/Voltage Range Status.
        /// </summary>
        Max1704XStatusEnVr = 1 << 14,

        /// <summary>
        /// MAX17043 Command Register Commands.
        /// </summary>
        Max17043CommandPor = 0x5400,

        /// <summary>
        /// MAX17048 Hibernate Enable/Disable.
        /// </summary>
        Max17048HibrtEnhib = 0xFFFF,
    }
}