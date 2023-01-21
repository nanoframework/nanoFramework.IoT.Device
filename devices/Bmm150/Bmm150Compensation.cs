// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Magnetometer;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Implements the Bmm150 magnetic field data (off-chip) temperature compensation functions
    /// https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmm150-ds001.pdf
    /// Page 15.
    /// </summary>
    public class Bmm150Compensation
    {
        /// <summary>
        /// Returns the compensated magnetometer x axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1614.
        /// </summary>
        /// <param name="x">Axis raw value.</param>
        /// <param name="rhall">Temperature compensation value (RHALL).</param>
        /// <param name="trimData">Trim registers values.</param>
        /// <returns>Compensated magnetometer x axis data(micro-tesla) in float.</returns>
        public static double CompensateX(double x, uint rhall, Bmm150TrimRegisterData trimData)
        {
            float retval = 0;
            float processCompX0;
            float processCompX1;
            float processCompX2;
            float processCompX3;
            float processCompX4;
            int bmm150OverflowAdcvalXyaxesFlip = -4096;

            // Overflow condition check
            if ((x != bmm150OverflowAdcvalXyaxesFlip) && (rhall != 0) && (trimData.DigXyz1 != 0))
            {
                // Processing compensation equations
                processCompX0 = trimData.DigXyz1 * 16384.0f / rhall;
                retval = processCompX0 - 16384.0f;
                processCompX1 = trimData.DigXy2 * (retval * retval / 268435456.0f);
                processCompX2 = processCompX1 + (retval * (trimData.DigXy1 / 16384.0f));
                processCompX3 = trimData.DigX2 + 160.0f;
                processCompX4 = (float)(x * ((processCompX2 + 256.0f) * processCompX3));
                retval = ((processCompX4 / 8192.0f) + (trimData.DigX1 * 8.0f)) / 16.0f;
            }
            else
            {
                // Overflow, set output to 0.0f
                retval = 0.0f;
            }

            return retval;
        }

        /// <summary>
        /// Returns the compensated magnetometer y axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1648.
        /// </summary>
        /// <param name="y">Axis raw value.</param>
        /// <param name="rhall">Temperature compensation value (RHALL).</param>
        /// <param name="trimData">Trim registers values.</param>
        /// <returns>Compensated magnetometer y axis data(micro-tesla) in float.</returns>
        public static double CompensateY(double y, uint rhall, Bmm150TrimRegisterData trimData)
        {
            float retval = 0;
            float processCompY0;
            float processCompY1;
            float processCompY2;
            float processCompY3;
            float processCompY4;
            int bmm150OverflowAdcvalXyaxesFlip = -4096;

            // Overflow condition check
            if ((y != bmm150OverflowAdcvalXyaxesFlip) && (rhall != 0) && (trimData.DigXyz1 != 0))
            {
                // Processing compensation equations
                processCompY0 = trimData.DigXyz1 * 16384.0f / rhall;
                retval = processCompY0 - 16384.0f;
                processCompY1 = trimData.DigXy2 * (retval * retval / 268435456.0f);
                processCompY2 = processCompY1 + (retval * (trimData.DigXy1 / 16384.0f));
                processCompY3 = trimData.DigY2 + 160.0f;
                processCompY4 = (float)(y * (((processCompY2)+256.0f) * processCompY3));
                retval = ((processCompY4 / 8192.0f) + (trimData.DigY1 * 8.0f)) / 16.0f;
            }
            else
            {
                // Overflow, set output to 0.0f
                retval = 0.0f;
            }

            return retval;
        }

        /// <summary>
        /// Returns the compensated magnetometer z axis data(micro-tesla) in float
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1682.
        /// </summary>
        /// <param name="z">Axis raw value.</param>
        /// <param name="rhall">Temperature compensation value (RHALL).</param>
        /// <param name="trimData">Trim registers values.</param>
        /// <returns>Compensated magnetometer z axis data(micro-tesla) in float.</returns>
        public static double CompensateZ(double z, uint rhall, Bmm150TrimRegisterData trimData)
        {
            float retval = 0;
            float processCompX0;
            float processCompX1;
            float processCompZ2;
            float processCompZ3;
            float processCompZ4;
            float processCompZ5;
            int bmm150OverflowAdcvalZaxisFlip = -16384;

            // Overflow condition check
            if ((z != bmm150OverflowAdcvalZaxisFlip) && (trimData.DigZ2 != 0) &&
                (trimData.DigZ1 != 0) && (trimData.DigXyz1 != 0) && (rhall != 0))
            {
                // Processing compensation equations
                processCompX0 = ((float)z) - trimData.DigZ4;
                processCompX1 = rhall - ((float)trimData.DigXyz1);
                processCompZ2 = trimData.DigZ3 * processCompX1;
                processCompZ3 = trimData.DigZ1 * ((float)rhall) / 32768.0f;
                processCompZ4 = trimData.DigZ2 + processCompZ3;
                processCompZ5 = (processCompX0 * 131072.0f) - processCompZ2;
                retval = (processCompZ5 / (processCompZ4 * 4.0f)) / 16.0f;
            }
            else
            {
                // Overflow, set output to 0.0f
                retval = 0.0f;
            }

            return retval;
        }
    }
}
