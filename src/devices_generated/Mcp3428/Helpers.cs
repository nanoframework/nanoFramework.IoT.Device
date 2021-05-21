// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp3428
{
    internal static class Helpers
    {
        /// <summary>
        /// Gets the voltage value corresponding to the least significant bit based on resolution.
        /// </summary>
        /// <param name="res">The resolution.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="ArgumentOutOfRangeException">res - null</exception>
        public static double LSBValue(AdcResolution res) => res switch
        {
            AdcResolution.Bit12 => 1e-3,
            AdcResolution.Bit14 => 250e-6,
            AdcResolution.Bit16 => 62.5e-6,
            _ => throw new ArgumentOutOfRangeException(nameof(res), res, null),
        };

        /// <summary>
        /// Gets the divisor to scale raw data based on resolution. = 1/LSB
        /// </summary>
        /// <param name="res">The resolution.</param>
        /// <returns>System.UInt16.</returns>
        /// <exception cref="ArgumentOutOfRangeException">res - null</exception>
        public static ushort LsbDivisor(AdcResolution res) => res switch
        {
            AdcResolution.Bit12 => 1000,
            AdcResolution.Bit14 => 4000,
            AdcResolution.Bit16 => 16000,
            _ => throw new ArgumentOutOfRangeException(nameof(res), res, null),
        };

        /// <summary>
        /// Determine device I2C address based on the configuration pin states. Based on documentation TABLE 5-3-
        /// </summary>
        /// <param name="adr0">The adr0 pin state</param>
        /// <param name="adr1">The adr1 pin state</param>
        /// <returns>System.Int32.</returns>
        public static byte I2CAddressFromPins(PinState adr0, PinState adr1)
        {
            byte addr = 0b1101000; // Base value from doc
            int idx = (byte)adr0 << 4 + (byte)adr1;
            int value = idx switch
            {
                0 or 0x22 => 0,
                0x02 => 1,
                0x01 => 2,
                0x10 => 4,
                0x12 => 5,
                0x11 => 6,
                0x20 => 3,
                0x21 => 7,
                _ => throw new ArgumentException("Invalid combination"),
            };

            return addr += (byte)value;
        }

        public static byte SetChannelBits(byte configByte, int channel)
        {
            if (channel > 3 || channel < 0)
            {
                throw new ArgumentException("Channel numbers are only valid 0 to 3", nameof(channel));
            }

            return (byte)((configByte & ~Helpers.Masks.ChannelMask) | ((byte)channel << 5));
        }

        public static byte SetGainBits(byte configByte, AdcGain gain) => (byte)((configByte & ~Helpers.Masks.GainMask) | (byte)gain);

        public static byte SetModeBit(byte configByte, AdcMode mode) => (byte)((configByte & ~Helpers.Masks.ModeMask) | (byte)mode);

        public static byte SetReadyBit(byte configByte, bool ready) => (byte)(ready ? configByte & ~Helpers.Masks.ReadyMask : configByte | Helpers.Masks.ReadyMask);

        public static byte SetResolutionBits(byte configByte, AdcResolution resolution) => (byte)((configByte & ~Helpers.Masks.ResolutionMask) | (byte)resolution);

        public static int UpdateFrequency(AdcResolution res) => res switch
        {
            AdcResolution.Bit12 => 240,
            AdcResolution.Bit14 => 60,
            AdcResolution.Bit16 => 15,
            _ => throw new ArgumentOutOfRangeException(nameof(res), res, null),
        };

        // From datasheet 5.2
        public static class Masks
        {
            public const byte ChannelMask = 0b01100000;
            public const byte GainMask = 0b00000011;
            public const byte ModeMask = 0b00010000;
            public const byte ReadyMask = 0b10000000;
            public const byte ResolutionMask = 0b00001100;
        }
    }
}
