////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Extension methods for <see cref="Dac63004"/> class to generate functions.
    /// </summary>
    public static class FunctionGeneratorExtensions
    {
        /// <summary>
        /// Generates a triangular wave with the specified parameters.
        /// </summary>
        /// <param name="dac">The <see cref="Dac63004"/> instance.</param>
        /// <param name="slewRate">The slew rate configuration for X function register.</param>
        /// <param name="stepSize"> The step size configuration from X function register.</param>
        /// <param name="lowMargin">The low margin of the wave. Value for DAC. From 0 to 4096.</param>
        /// <param name="highMargin">The high margin of the wave.Value for DAC. From 0 to 4096.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lowMargin"/> or <paramref name="highMargin"/> are out of range.</exception>
        /// <remarks>
        /// More information about the function generator can be found in the datasheet section "7.4.5.2.1 Triangular Waveform Generation".
        /// </remarks>
        public static void GenerateTriangularWave(
            this Dac63004 dac,
            SlewRate slewRate,
            CodeStep stepSize,
            int lowMargin,
            int highMargin)
        {
            if (lowMargin < 0
               || lowMargin > 4096
               || highMargin < 0
               || highMargin > 4096)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] writeBuffer = new byte[2];

            // write DAC-X-MARGIN-HIGH
            writeBuffer[0] = (byte)(highMargin >> 4);
            writeBuffer[1] = (byte)(highMargin << 4);
            dac.WriteToRegister(Register.Reg01_DACX_MarginHigh, writeBuffer);

            // write DAC-X-MARGIN-LOW
            writeBuffer[0] = (byte)(lowMargin >> 4);
            writeBuffer[1] = (byte)(lowMargin << 4);
            dac.WriteToRegister(Register.Reg02_DACX_MarginLow, writeBuffer);

            // write DAC-X-FUNC-CONFIG
            ushort funcConfig = (ushort)((ushort)Wave.Triangular | (ushort)slewRate | (ushort)stepSize);
            writeBuffer[0] = (byte)(funcConfig >> 8);
            writeBuffer[1] = (byte)funcConfig;
            dac.WriteToRegister(Register.Reg06_DACX_FunctionConfig, writeBuffer);
        }

        /// <summary>
        /// Generates a sine wave with the specified parameters.
        /// </summary>
        /// <param name="dac"> The <see cref="Dac63004"/> instance.</param>
        /// <param name="slewRate">The slew rate configuration for X function register.</param>
        /// <param name="stepSize">The step size configuration from X function register.</param>
        /// <param name="lowMargin">The low margin of the wave. Value for DAC. From 0 to 4096.</param>
        /// <param name="highMargin">The high margin of the wave. Value for DAC. From 0 to 4096.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lowMargin"/> or <paramref name="highMargin"/> are out of range.</exception>
        public static void GenerateSawtoothWave(
            this Dac63004 dac,
            SlewRate slewRate,
            CodeStep stepSize,
            int lowMargin,
            int highMargin)
        {
            if (lowMargin < 0
               || lowMargin > 4096
               || highMargin < 0
               || highMargin > 4096)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] writeBuffer = new byte[2];

            // write DAC-X-MARGIN-HIGH
            writeBuffer[0] = (byte)(highMargin >> 4);
            writeBuffer[1] = (byte)(highMargin << 4);
            dac.WriteToRegister(Register.Reg01_DACX_MarginHigh, writeBuffer);

            // write DAC-X-MARGIN-LOW
            writeBuffer[0] = (byte)(lowMargin >> 4);
            writeBuffer[1] = (byte)(lowMargin << 4);
            dac.WriteToRegister(Register.Reg02_DACX_MarginLow, writeBuffer);

            // write DAC-X-FUNC-CONFIG
            ushort funcConfig = (ushort)((ushort)Wave.Sawtooth | (ushort)slewRate | (ushort)stepSize);
            writeBuffer[0] = (byte)(funcConfig >> 8);
            writeBuffer[1] = (byte)funcConfig;
            dac.WriteToRegister(Register.Reg06_DACX_FunctionConfig, writeBuffer);
        }

        /// <summary>
        /// Generates a sine wave with the specified parameters.
        /// </summary>
        /// <param name="dac"> The <see cref="Dac63004"/> instance.</param>
        /// <param name="slewRate">The slew rate configuration for X function register.</param>
        /// <remarks>
        /// More information about the function generator can be found in the datasheet section "7.4.5.2.1 Triangular Waveform Generation".
        /// </remarks>
        public static void GenerateSineWave(
            this Dac63004 dac,
            SlewRate slewRate)
        {
            byte[] writeBuffer = new byte[2];

            // write DAC-X-FUNC-CONFIG
            ushort funcConfig = (ushort)((ushort)Wave.Sine | (ushort)slewRate);
            writeBuffer[0] = (byte)(funcConfig >> 8);
            writeBuffer[1] = (byte)funcConfig;
            dac.WriteToRegister(Register.Reg06_DACX_FunctionConfig, writeBuffer);
        }
    }
}
