// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc
{
    /// <summary>
    /// INA236 operating modes.
    /// </summary>
    public enum Ina236OperatingMode
    {
        /// <summary>The device is shut down.</summary>
        Shutdown = 0b000,

        /// <summary>Performs one shunt-voltage measurement.</summary>
        SingeShuntVoltage = 0b001,

        /// <summary>Performs one shunt-voltage measurement.</summary>
        SingleShuntVoltage = SingeShuntVoltage,

        /// <summary>Performs one bus-voltage measurement.</summary>
        SingleBusVoltage = 0b010,

        /// <summary>Performs one shunt- and bus-voltage measurement.</summary>
        SingleShuntAndBusVoltage = 0b011,

        /// <summary>The alternate shutdown mode.</summary>
        Shutdown2 = 0b100,

        /// <summary>Continuously measures shunt voltage.</summary>
        ContinuousShuntVoltage = 0b101,

        /// <summary>Continuously measures bus voltage.</summary>
        ContinuousBusVoltage = 0b110,

        /// <summary>Continuously measures shunt and bus voltage.</summary>
        ContinuousShuntAndBusVoltage = 0b111,

        /// <summary>The operating-mode bit mask.</summary>
        ModeMask = 0b111
    }
}