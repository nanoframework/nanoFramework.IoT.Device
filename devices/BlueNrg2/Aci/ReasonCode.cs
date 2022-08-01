// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Reason code.
    /// </summary>
    public enum ReasonCode : byte
    {
        /// <summary>
        /// Firmware started properly.
        /// </summary>
        FirmwareStartedProperly = 0x01,

        /// <summary>
        /// Updater mode entered with ACI command.
        /// </summary>
        UpdaterModeWithAciCommand = 0x02,

        /// <summary>
        /// Updater mode entered due to bad Blue Flag.
        /// </summary>
        UpdaterModeBadBlueFlag = 0x03,

        /// <summary>
        /// Updater mode entered due to IRQ pin.
        /// </summary>
        UpdaterModeIrqPin = 0x04,

        /// <summary>
        /// System reset due to watchdog.
        /// </summary>
        SystemResetWatchdog = 0x05,

        /// <summary>
        /// System reset due to lockup.
        /// </summary>
        SystemResetLockup = 0x06,

        /// <summary>
        /// System reset due to brownout reset.
        /// </summary>
        SystemResetBrownout = 0x07,

        /// <summary>
        /// System reset due to crash.
        /// </summary>
        SystemResetCrash = 0x08,

        /// <summary>
        /// System reset due to ECC error.
        /// </summary>
        SystemResetEccError = 0x09
    }
}
