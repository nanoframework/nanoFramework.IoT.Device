// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Drivers.Ssd1681
{
    /// <summary>
    /// Commands supported by SSD1681.
    /// </summary>
    public enum Command : byte
    {
        /// <summary>
        /// Driver Output Control Command. Sets the gate, scanning order, etc.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        DriverOutputControl = 0x01,

        /// <summary>
        /// Gate Driving Voltage Command.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        GateDrivingVoltage = 0x03,

        /// <summary>
        /// Source Driving Voltage Control.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        SourceDrivingVoltageControl = 0x04,

        /// <summary>
        /// Program Initial Code Setting.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        InitialCodeSettingOtpProgram = 0x08,

        /// <summary>
        /// Write Register for Initial Code Setting Selection.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteRegisterForInitialCodeSetting = 0x09,

        /// <summary>
        /// Read Register for Initial Code Setting.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ReadRegisterForInitialCodeSetting = 0x0a,

        /// <summary>
        /// Booster Enable with Phase 1, Phase 2 and Phase 3 for soft start current and duration setting.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        BoosterSoftStartControl = 0x0c,

        /// <summary>
        /// Set Deep Sleep Mode.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        DeepSleepMode = 0x10,

        /// <summary>
        /// Define data entry sequence.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        DataEntryModeSetting = 0x11,

        /// <summary>
        /// Software Reset. This resets the commands and parameters to their S/W default values except Deep Sleep Mode.
        /// The Busy pin will read high during this operation.
        /// RAM contents are not affected by this command.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        SoftwareReset = 0x12,

        /// <summary>
        /// HV Ready Detection.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        HvReadyDetection = 0x14,

        /// <summary>
        /// VCI Detection.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        VciDetection = 0x15,

        /// <summary>
        /// Temperature Sensor Selection (External vs Internal).
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        TempSensorControlSelection = 0x18,

        /// <summary>
        /// Write to temperature register.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        TempSensorControlWriteRegister = 0x1a,

        /// <summary>
        /// Read from temperature register.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        TempSensorControlReadRegister = 0x1b,

        /// <summary>
        /// Temperature Sensor Control. Write command to External temperature sensor.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ExternalTempSensorControlWrite = 0x1c,

        /// <summary>
        /// Master Activation. Activate display update sequence.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        MasterActivation = 0x20,

        /// <summary>
        /// Display Update Control 1.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        DisplayUpdateControl1 = 0x21,

        /// <summary>
        /// Display Update Sequence Option. Enables the stage for <see cref="MasterActivation"/>.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        DisplayUpdateControl2 = 0x22,

        /// <summary>
        /// Write To B/W RAM. After this command, data will be written to the B/W RAM until another command is sent.
        /// Address pointers will advance accordingly.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteBackWhiteRAM = 0x24,

        /// <summary>
        /// Write To RED RAM. After this command, data will be written to the RED RAM until another command is sent.
        /// Address pointers will advance accordingly.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteRedRAM = 0x26,

        /// <summary>
        /// Read RAM. After this command, data read on the MCU bus will fetch data from RAM.
        /// The first byte read is dummy data.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ReadRAM = 0x27,

        /// <summary>
        /// Enter VCOM sensing conditions.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        VCOMSense = 0x28,

        /// <summary>
        /// VCOM Sense Duration. Stabling time between entering VCOM sending mode and reading is acquired.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        VCOMSenseDuration = 0x29,

        /// <summary>
        /// Program VCOM register into OTP.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ProgramVCOMOTP = 0x2a,

        /// <summary>
        /// Write Register for VCOM Control. This command is used to reduce glitch when ACVCOM toggle.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteRegisterControlVCOM = 0x2b,

        /// <summary>
        /// Write VCOM register from MCU interface.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteVCOMRegister = 0x2c,

        /// <summary>
        /// OTP Register Read for Display Options.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        OTPRegisterReadDisplayOption = 0x2d,

        /// <summary>
        /// Read USER ID.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        UserIdRead = 0x2e,

        /// <summary>
        /// Read Status Bit.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        StatusBitRead = 0x2f,

        /// <summary>
        /// Program Waveform Setting OTP. Contents should be written to RAM before sending this command.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ProgramWSOTP = 0x30,

        /// <summary>
        /// Load OTP of Waveform Setting.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        LoadWSOTP = 0x31,

        /// <summary>
        /// Write LUT Register from MCU interface.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteLUTRegister = 0x32,

        /// <summary>
        /// CRC Calculation Command. For information, refer to SSD1681 application note.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        CrcCalculation = 0x34,

        /// <summary>
        /// CRC Status Read.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        CrcStatusRead = 0x35,

        /// <summary>
        /// Program OTP Selection according to the <see cref="WriteRegisterForDisplayOption"/> and <see cref="WriteRegisterForUserId"/>.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ProgramOTPSelection = 0x36,

        /// <summary>
        /// Write Register for Display Option.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteRegisterForDisplayOption = 0x37,

        /// <summary>
        /// Write Register for USER ID.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        WriteRegisterForUserId = 0x38,

        /// <summary>
        /// OTP Program Mode.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        OTPProgramMode = 0x39,

        /// <summary>
        /// Set Border Waveform Control values.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        BorderWaveformControl = 0x3c,

        /// <summary>
        /// Option for LUT end.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        EndOption = 0x3f,

        /// <summary>
        /// Read RAM Option.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        ReadRAMOption = 0x41,

        /// <summary>
        /// Set RAM X-Address Start/End position.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        SetRAMAddressXStartEndPosition = 0x44,

        /// <summary>
        /// Set RAM Y-Address Start/End position.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        SetRAMAddressYStartEndPosition = 0x45,

        /// <summary>
        /// Auto Write RED RAM for regular pattern.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        AutoWriteRAMForRegularPatternRed = 0x46,

        /// <summary>
        /// Auto Write B/W RAM for regular pattern.
        /// The Busy pin will read high during this operation.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        AutoWriteRAMForRegularPatternBlackWhite = 0x47,

        /// <summary>
        /// Set RAM X-Address Counter.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        SetRAMAddressCounterX = 0x4e,

        /// <summary>
        /// Set RAM Y-Address Counter.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        SetRAMAddressCounterY = 0x4f,

        /// <summary>
        /// NOP. This command is an empty command; it does not have any effect on the display module.
        /// However, it can be used to terminate frame memory write or read commands.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        NOP = 0x7f
    }
}
