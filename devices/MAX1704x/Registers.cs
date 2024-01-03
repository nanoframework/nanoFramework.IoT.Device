// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// The Registers class contains constant values representing memory addresses of various registers in the MAX17043 IC.
    /// </summary>
    public enum Registers : byte
    {
        /// <summary>
        /// R - 12-bit A/D measurement of battery voltage.
        /// </summary>
        Max17043Vcell = 0x02,

        /// <summary>
        /// R - 16-bit state of charge (SOC).
        /// </summary>
        Max17043Soc = 0x04,

        /// <summary>
        /// W - Sends special commands to IC.
        /// </summary>
        Max17043Mode = 0x06,
        
        /// <summary>
        /// R - Returns IC version.
        /// </summary>
        Max17043Version = 0x08,
        
        /// <summary>
        /// R/W - (MAX17048/49) Thresholds for entering hibernate.
        /// </summary>
        Max17048Hibrt = 0x0A,
        
        /// <summary>
        /// R/W - Battery compensation (default 0x971C).
        /// </summary>
        Max17043Config = 0x0C, 

        /// <summary>
        /// R/W - (MAX17048/49) Configures vcell range to generate alerts (default 0x00FF).
        /// </summary>
        Max17048Cvalrt = 0x14, 

        /// <summary>
        /// R - (MAX17048/49) Charge rate 0.208%/hr.
        /// </summary>
        Max17048Crate = 0x16,
        
        /// <summary>
        /// R/W - (MAX17048/49) Reset voltage and ID (default 0x96__).
        /// </summary>
        Max17048VresetId = 0x18,
        
        /// <summary>
        /// R/W - (MAX17048/49) Status of ID (default 0x01__).
        /// </summary>
        Max17048Status = 0x1A,
        
        /// <summary>
        /// W - Sends special comands to IC.
        /// </summary>
        Max17043Command = 0xFE,

        /// <summary>
        /// The Registers class contains constant values representing memory addresses of various registers in the MAX17043 IC.
        /// </summary>
        Max17043ConfigSleep = 1 << 7,

        /// <summary>
        /// MAX17048/49 only.
        /// </summary>
        Max17043ConfigAlsc = 0x0040,
        
        /// <summary>
        /// MAX17048/49 only.
        /// </summary>
        Max17043ConfigAlert = 1 << 5,
        
        /// <summary>
        /// MAX17048/9 Status Register Bits.
        /// </summary>
        Max1704XStatusRi = 1 << 0,
        
        /// <summary>
        /// MAX17048/9 Status Register Bits.
        /// </summary>
        Max1704XStatusVh = 1 << 1,
        
        /// <summary>
        /// MAX17048/9 Status Register Bits.
        /// </summary>
        Max1704XStatusVl = 1 << 2,
        
        /// <summary>
        /// MAX17048/9 Status Register Bits.
        /// </summary>
        Max1704XStatusVr = 1 << 3,
        
        /// <summary>
        /// MAX17048/9 Status Register Bits.
        /// </summary>
        Max1704XStatusHd = 1 << 4,
        
        /// <summary>
        /// MAX17048/9 Status Register Bits.
        /// </summary>
        Max1704XStatusSc = 1 << 5,
        
        /// <summary>
        /// Disable hibernate mode.
        /// </summary>
        Max17048HibrtDishib = 0x0000, 
    }

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