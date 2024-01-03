// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// The Registers class contains constant values representing memory addresses of various registers in the MAX17043 IC.
    /// </summary>
    internal static class Registers
    {
        // R - 12-bit A/D measurement of battery voltage
        internal const byte Max17043Vcell = 0x02;

        // R - 16-bit state of charge (SOC)
        internal const byte Max17043Soc = 0x04;

        // W - Sends special commands to IC
        internal const byte Max17043Mode = 0x06;
        
        // R - Returns IC version
        internal const byte Max17043Version = 0x08;
        
        // R/W - (MAX17048/49) Thresholds for entering hibernate
        internal const byte Max17048Hibrt = 0x0A;
        
        // R/W - Battery compensation (default 0x971C)
        internal const byte Max17043Config = 0x0C; 

        // R/W - (MAX17048/49) Configures vcell range to generate alerts (default 0x00FF)
        internal const byte Max17048Cvalrt = 0x14; 

        // R - (MAX17048/49) Charge rate 0.208%/hr
        internal const byte Max17048Crate = 0x16;
        
        // R/W - (MAX17048/49) Reset voltage and ID (default 0x96__)
        internal const byte Max17048VresetId = 0x18;
        
        // R/W - (MAX17048/49) Status of ID (default 0x01__)
        internal const byte Max17048Status = 0x1A;
        
        // W - Sends special comands to IC
        internal const byte Max17043Command = 0xFE; 

        internal const byte Max17043ConfigSleep = 1 << 7;

        // MAX17048/49 only
        internal const byte Max17043ConfigAlsc = 0x0040;
        internal const byte Max17043ConfigAlert = 1 << 5;
        
        // On the MAX17048/49 this also clears the EnSleep bit
        internal const ushort Max17043ModeQuickstart = 0x4000; 

        // W - _Enables_ sleep mode (the SLEEP bit in the CONFIG reg engages sleep)
        internal const ushort Max17048ModeEnsleep = 0x2000;
        
        // R - indicates when the IC is in hibernate mode
        internal const ushort Max17048ModeHibstat = 0x1000;

        /////////////////////////////////////
        // MAX17048/9 Status Register Bits //
        /////////////////////////////////////
        internal const byte Max1704XStatusRi = 1 << 0; 
        internal const byte Max1704XStatusVh = 1 << 1;
        internal const byte Max1704XStatusVl = 1 << 2;
        internal const byte Max1704XStatusVr = 1 << 3;
        internal const byte Max1704XStatusHd = 1 << 4;
        internal const byte Max1704XStatusSc = 1 << 5;
        internal const ushort Max1704XStatusEnVr = 1 << 14;

        ////////////////////////////////////////
        // MAX17043 Command Register Commands //
        ////////////////////////////////////////
        internal const ushort Max17043CommandPor = 0x5400;

        ///////////////////////////////////////
        // MAX17048 Hibernate Enable/Disable //
        ///////////////////////////////////////
        // always use hibernate mode
        internal const ushort Max17048HibrtEnhib = 0xFFFF;
        
        // disable hibernate mode
        internal const byte Max17048HibrtDishib = 0x0000; 
    }
}