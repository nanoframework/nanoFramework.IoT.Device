// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents SetComPinsHardwareConfiguration command.
    /// </summary>
    public class SetComPinsHardwareConfiguration : ISsd1306Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetComPinsHardwareConfiguration" /> class.
        /// This command sets the COM signals pin configuration to match the OLED panel hardware layout.
        /// </summary>
        /// <param name="alternativeComPinConfiguration">Alternative COM pin configuration.</param>
        /// <param name="enableLeftRightRemap">Enable left/right remap.</param>
        public SetComPinsHardwareConfiguration(bool alternativeComPinConfiguration = true, bool enableLeftRightRemap = false)
        {
            AlternativeComPinConfiguration = alternativeComPinConfiguration;
            EnableLeftRightRemap = enableLeftRightRemap;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xDA;

        /// <summary>
        /// Gets a value indicating whether alternative COM pin configuration is enabled.
        /// </summary>
        public bool AlternativeComPinConfiguration { get; }

        /// <summary>
        /// Gets a value indicating whether enable left/right remap.
        /// </summary>
        public bool EnableLeftRightRemap { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            byte comPinsHardwareConfiguration = 0x02;

            if (AlternativeComPinConfiguration)
            {
                comPinsHardwareConfiguration |= 0x10;
            }

            if (EnableLeftRightRemap)
            {
                comPinsHardwareConfiguration |= 0x20;
            }

            return new byte[] { Id, comPinsHardwareConfiguration };
        }
    }
}