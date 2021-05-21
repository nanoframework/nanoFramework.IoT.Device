// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// This class contains utilities for working with AMG88xx devices.
    /// </summary>
    internal static class Amg88xxUtils
    {
        /// <summary>
        /// Temperature resolution of a pixel (in degrees celsius)
        /// </summary>
        private const double PixelTemperatureResolution = 0.25;

        /// <summary>
        /// Converts a temperature from 12-bit (in 2 bytes) two's complements representation into a floating-point reading.
        /// </summary>
        /// <param name="twosComplement">Reading in two's complement little endian representation</param>
        /// <returns>Temperature reading</returns>
        public static Temperature ConvertToTemperature(SpanByte twosComplement) => ConvertToTemperature(twosComplement[0], twosComplement[1]);

        /// <summary>
        /// Converts a temperature from two's complements representation into a floating-point reading.
        /// </summary>
        /// <param name="tl">Reading low byte</param>
        /// <param name="th">Reading high byte</param>
        /// <returns>Temperature reading</returns>
        public static Temperature ConvertToTemperature(byte tl, byte th)
        {
            // The temperature of each pixel is encoded as a 12 bit two's complement value.
            int reading = (th & 0x7) << 8 | tl;
            reading = th >> 3 == 0 ? reading : -(~(reading - 1) & 0x7ff);
            // The LSB is equivalent to 0.25℃
            return Temperature.FromDegreesCelsius(reading * PixelTemperatureResolution);
        }

        /// <summary>
        /// Converts a temperature from floating-point representation into a two's complement representation (low- and high-byte).
        /// </summary>
        /// <param name="temperature">Temperature </param>
        /// <returns>Two's complement representation</returns>
        public static (byte LowByte, byte HighByte) ConvertFromTemperature(Temperature temperature)
        {
            // The temperature of each pixel is encoded as a 12 bit value in two's complement form.
            // The LSB is equivalent to 0.25℃
            var t = (int)(temperature.DegreesCelsius / PixelTemperatureResolution);
            t = temperature.DegreesCelsius < 0 ? ~(0x1000 - t) + 1 : t;
            return ((byte)(t & 0xff), (byte)((t >> 8) & 0x0f));
        }
    }
}
