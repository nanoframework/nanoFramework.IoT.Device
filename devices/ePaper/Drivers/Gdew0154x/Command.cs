// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Drivers.GDEW0154x
{
    /// <summary>
    /// Commands supported by SSD1681.
    /// </summary>
    public enum Command : byte
    {
        /// <summary>
        /// Sets the settings for the screen.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        PanelSetting = 0x00,

        /// <summary>
        /// Power off command.
        /// </summary>
        /// <remarks>
        /// After power off command, driver will power off base on power off sequence.
        /// After power off command, BUSY_N signal will drop from high to low.
        /// When finish the power off sequence, BUSY_N singal will rise from low to high.
        /// Power off command will turn off charge pump, T-con, source driver, gate driver, VCOM,
        /// temperature sensor, but register and SRAM data will keep until VDD off.
        /// </remarks>
        PowerOff = 0x02,

        /// <summary>
        /// Power on command.
        /// </summary>
        /// <remarks>
        /// After power on command, driver will power on base on power on sequence.
        /// After power on command, BUSY_N signal will drop from high to low.
        /// When finishing the power on sequence, BUSY_N signal will rise from low to high.
        /// </remarks>
        PowerOn = 0x04,

        /// <summary>
        /// Sets the screen in deep sleep mode.
        /// </summary>
        /// <remarks>
        /// The command define as follows: After this command is transmitted, the chip would
        /// enter the deep-sleep mode to save power.
        /// The deep sleep mode would return to standby by hardware reset.
        /// </remarks>
        DeepSleepMode = 0x07,

        /// <summary>
        /// Start a data pixel value writing.
        /// </summary>
        /// <remarks>
        /// The command define as follows:
        /// The register is indicates that user start to transmit data, then write to SRAM.
        /// While data transmission complete, user must send command 11H.
        /// Then chip will start to send data/VCOM for panel.
        /// In BW mode, this command writes “OLD” data to SRAM.
        /// In BWR mode, this command writes “B/W” data to SRAM.
        /// In Program mode, this command writes “OTP” data to SRAM for programming.
        /// </remarks>
        DataStartTransmission1 = 0x10,

        /// <summary>
        /// Sets the settings for the screen.
        /// </summary>
        /// <remarks>
        /// The command defines as :
        /// While users send this command, driver will refresh display
        /// (data/VCOM) base on SRAM data and LUT.
        /// After display refresh command, BUSY_N signal will become “0”.
        /// </remarks>
        DisplayRefresh = 0x12,

        /// <summary>
        /// Start a data pixel value writing.
        /// </summary>
        /// <remarks>
        /// The command define as follows:
        /// The register is indicates that user start to transmit data, then write to SRAM.
        /// While data transmission complete, user must send command 11H.
        /// Then chip will start to send data/VCOM for panel.
        /// In B/W mode, this command writes “NEW” data to SRAM.
        /// In B/W/Red mode, this command writes “RED” data to SRAM.
        /// </remarks>
        DataStartTransmission2 = 0x13,

        /// <summary>
        /// Defines non-overlap period of Gate and Source.
        /// </summary>
        /// <remarks>
        /// Refer to the datasheet for detailed information about the command and its parameters.
        /// </remarks>
        TCONSetting = 0x60,

        /// <summary>
        /// Define screen resolution setting.
        /// </summary>
        /// <remarks>
        /// This command defines as follow:
        /// HRES[7:3]: Horizontal Display Resolution (first data)
        /// VRES[8:0]: Vertical Display Resolution (second and third data)
        /// Active channel calculation:
        ///     - GD: First active gate = G0(Fixed); LAST active gate = first active + VRES[8:0] – 1
        ///     - SD: First active source = S0(Fixed); LAST active source = first active+HRES[7:3]*8–1
        /// </remarks>
        ResolutionSetting = 0x61,

        /// <summary>
        /// Defines power saving settings.
        /// </summary>
        /// <remarks>
        /// This command is set for saving power during fresh period.
        /// If the output voltage of VCOM / Source is from negative to positive or from positive to negative,
        /// the power saving mechanism will be activated.
        /// The active period width is defined by the following two parameters.
        ///     - VCOM_W[3:0]: VCOM power saving width (unit = line period)
        ///     - SD_W[3:0]: Source power saving width (unit = 660nS)
        /// </remarks>
        PowerSaving = 0xe3
    }
}
